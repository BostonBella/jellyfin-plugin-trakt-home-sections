using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Jellyfin.Plugin.HomeScreenSections.Model.Trakt;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.Services
{
    /// <summary>
    /// Service for interacting with the Trakt API.
    /// </summary>
    public class TraktService
    {
        private const string c_traktApiBaseUrl = "https://api.trakt.tv";

        private readonly ILogger<TraktService> m_logger;
        private readonly IHttpClientFactory m_httpClientFactory;
        private readonly JsonSerializerOptions m_jsonOptions;

        public TraktService(ILogger<TraktService> logger, IHttpClientFactory httpClientFactory)
        {
            m_logger = logger;
            m_httpClientFactory = httpClientFactory;
            m_jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
        }

        private static string TraktClientId => HomeScreenSectionsPlugin.Instance.Configuration.TraktClientId ?? string.Empty;

        private static string TraktClientSecret => HomeScreenSectionsPlugin.Instance.Configuration.TraktClientSecret ?? string.Empty;

        private async Task<HttpClient> CreateTraktClient()
        {
            await EnsureValidToken();

            var httpClient = m_httpClientFactory.CreateClient(NamedClient.Default);
            httpClient.BaseAddress = new Uri(c_traktApiBaseUrl);
            httpClient.DefaultRequestHeaders.Add("trakt-api-version", "2");

            if (!string.IsNullOrWhiteSpace(TraktClientId))
            {
                httpClient.DefaultRequestHeaders.Add("trakt-api-key", TraktClientId);
            }

            var config = HomeScreenSectionsPlugin.Instance.Configuration;
            if (!string.IsNullOrWhiteSpace(config.TraktAccessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", config.TraktAccessToken);
            }

            return httpClient;
        }

        private async Task EnsureValidToken()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;

            if (string.IsNullOrWhiteSpace(config.TraktRefreshToken))
            {
                return;
            }

            if (DateTime.Now >= config.TraktTokenExpiration)
            {
                await RefreshAccessToken();
            }
        }

        private async Task RefreshAccessToken()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;

            if (string.IsNullOrWhiteSpace(TraktClientId) || string.IsNullOrWhiteSpace(TraktClientSecret))
            {
                m_logger.LogWarning("Cannot refresh Trakt token because client ID or client secret is missing.");
                return;
            }

            var request = new
            {
                refresh_token = config.TraktRefreshToken,
                client_id = TraktClientId,
                client_secret = TraktClientSecret,
                grant_type = "refresh_token"
            };

            try
            {
                var httpClient = m_httpClientFactory.CreateClient(NamedClient.Default);
                httpClient.BaseAddress = new Uri(c_traktApiBaseUrl);
                httpClient.DefaultRequestHeaders.Add("trakt-api-version", "2");
                httpClient.DefaultRequestHeaders.Add("trakt-api-key", TraktClientId);

                var response = await httpClient.PostAsJsonAsync("/oauth/token", request);
                response.EnsureSuccessStatusCode();

                var token = await response.Content.ReadFromJsonAsync<TraktAccessToken>(m_jsonOptions);
                if (token == null)
                {
                    return;
                }

                config.TraktAccessToken = token.AccessToken;
                config.TraktRefreshToken = token.RefreshToken;
                config.TraktTokenExpiration = DateTime.Now.AddSeconds(token.ExpiresIn * 0.75);
                HomeScreenSectionsPlugin.Instance.SaveConfiguration();

                m_logger.LogInformation("Successfully refreshed Trakt access token.");
            }
            catch (Exception ex)
            {
                m_logger.LogError(ex, "Failed to refresh Trakt access token.");
            }
        }

        public async Task<TraktDeviceCode?> AuthorizeDevice()
        {
            if (string.IsNullOrWhiteSpace(TraktClientId))
            {
                m_logger.LogWarning("Cannot authorize Trakt device because client ID is missing.");
                return null;
            }

            var request = new { client_id = TraktClientId };

            var httpClient = m_httpClientFactory.CreateClient(NamedClient.Default);
            httpClient.BaseAddress = new Uri(c_traktApiBaseUrl);
            httpClient.DefaultRequestHeaders.Add("trakt-api-version", "2");
            httpClient.DefaultRequestHeaders.Add("trakt-api-key", TraktClientId);

            var response = await httpClient.PostAsJsonAsync("/oauth/device/code", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TraktDeviceCode>(m_jsonOptions);
        }

        public async Task<bool> PollForToken(TraktDeviceCode deviceCode)
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;

            if (string.IsNullOrWhiteSpace(TraktClientId) || string.IsNullOrWhiteSpace(TraktClientSecret))
            {
                m_logger.LogWarning("Cannot poll for Trakt token because client ID or client secret is missing.");
                return false;
            }

            var request = new
            {
                code = deviceCode.DeviceCode,
                client_id = TraktClientId,
                client_secret = TraktClientSecret
            };

            var expiresAt = DateTime.UtcNow.AddSeconds(deviceCode.ExpiresIn);

            var httpClient = m_httpClientFactory.CreateClient(NamedClient.Default);
            httpClient.BaseAddress = new Uri(c_traktApiBaseUrl);
            httpClient.DefaultRequestHeaders.Add("trakt-api-version", "2");
            httpClient.DefaultRequestHeaders.Add("trakt-api-key", TraktClientId);

            while (DateTime.UtcNow < expiresAt)
            {
                var response = await httpClient.PostAsJsonAsync("/oauth/device/token", request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var token = await response.Content.ReadFromJsonAsync<TraktAccessToken>(m_jsonOptions);
                    if (token == null)
                    {
                        return false;
                    }

                    config.TraktAccessToken = token.AccessToken;
                    config.TraktRefreshToken = token.RefreshToken;
                    config.TraktTokenExpiration = DateTime.Now.AddSeconds(token.ExpiresIn * 0.75);
                    HomeScreenSectionsPlugin.Instance.SaveConfiguration();

                    m_logger.LogInformation("Successfully authorized Trakt account.");
                    return true;
                }

                if (response.StatusCode == HttpStatusCode.Gone || response.StatusCode == (HttpStatusCode)418)
                {
                    return false;
                }

                await Task.Delay(deviceCode.Interval * 1000);
            }

            return false;
        }

        public async Task<TraktListItem[]> GetListItems(string listId, int limit = 20)
        {
            using var httpClient = await CreateTraktClient();
            var response = await httpClient.GetAsync($"/users/me/lists/{listId}/items?extended=full&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to get Trakt list items for list {ListId}: {Status}", listId, response.StatusCode);
                return Array.Empty<TraktListItem>();
            }

            var items = await response.Content.ReadFromJsonAsync<TraktListItem[]>(m_jsonOptions);
            return items ?? Array.Empty<TraktListItem>();
        }

        public async Task<TraktListItem[]> GetWatchlist(int limit = 20)
        {
            using var httpClient = await CreateTraktClient();

            var result = new List<TraktListItem>();

            var moviesResponse = await httpClient.GetAsync($"/sync/watchlist/movies?extended=full&limit={limit}");
            if (moviesResponse.IsSuccessStatusCode)
            {
                var movies = await moviesResponse.Content.ReadFromJsonAsync<TraktWatchlistMovieItem[]>(m_jsonOptions);
                if (movies != null)
                {
                    foreach (var movie in movies)
                    {
                        result.Add(new TraktListItem { Type = "movie", Movie = movie.Movie });
                    }
                }
            }

            var showsResponse = await httpClient.GetAsync($"/sync/watchlist/shows?extended=full&limit={limit}");
            if (showsResponse.IsSuccessStatusCode)
            {
                var shows = await showsResponse.Content.ReadFromJsonAsync<TraktWatchlistShowItem[]>(m_jsonOptions);
                if (shows != null)
                {
                    foreach (var show in shows)
                    {
                        result.Add(new TraktListItem { Type = "show", Show = show.Show });
                    }
                }
            }

            return result.ToArray();
        }

        public async Task<TraktMovie[]> GetMovieRecommendations(int limit = 20)
        {
            using var httpClient = await CreateTraktClient();
            var response = await httpClient.GetAsync($"/recommendations/movies?extended=full&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to get Trakt movie recommendations: {Status}", response.StatusCode);
                return Array.Empty<TraktMovie>();
            }

            var movies = await response.Content.ReadFromJsonAsync<TraktMovie[]>(m_jsonOptions);
            return movies ?? Array.Empty<TraktMovie>();
        }

        public async Task<TraktShow[]> GetShowRecommendations(int limit = 20)
        {
            using var httpClient = await CreateTraktClient();
            var response = await httpClient.GetAsync($"/recommendations/shows?extended=full&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to get Trakt show recommendations: {Status}", response.StatusCode);
                return Array.Empty<TraktShow>();
            }

            var shows = await response.Content.ReadFromJsonAsync<TraktShow[]>(m_jsonOptions);
            return shows ?? Array.Empty<TraktShow>();
        }

        public async Task<TraktMovie[]> GetTrendingMovies(int limit = 20)
        {
            using var httpClient = await CreateTraktClient();
            var response = await httpClient.GetAsync($"/movies/trending?extended=full&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to get Trakt trending movies: {Status}", response.StatusCode);
                return Array.Empty<TraktMovie>();
            }

            var trendingItems = await response.Content.ReadFromJsonAsync<TraktTrendingMovieItem[]>(m_jsonOptions);
            if (trendingItems == null)
            {
                return Array.Empty<TraktMovie>();
            }

            return trendingItems.Select(item => item.Movie).ToArray();
        }

        public async Task<TraktShow[]> GetTrendingShows(int limit = 20)
        {
            using var httpClient = await CreateTraktClient();
            var response = await httpClient.GetAsync($"/shows/trending?extended=full&limit={limit}");

            if (!response.IsSuccessStatusCode)
            {
                m_logger.LogError("Failed to get Trakt trending shows: {Status}", response.StatusCode);
                return Array.Empty<TraktShow>();
            }

            var trendingItems = await response.Content.ReadFromJsonAsync<TraktTrendingShowItem[]>(m_jsonOptions);
            if (trendingItems == null)
            {
                return Array.Empty<TraktShow>();
            }

            return trendingItems.Select(item => item.Show).ToArray();
        }
    }
}
