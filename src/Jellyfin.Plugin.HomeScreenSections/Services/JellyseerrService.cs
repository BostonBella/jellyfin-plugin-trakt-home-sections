using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.HomeScreenSections.Services;

/// <summary>
/// Represents a resolved Jellyseerr media item.
/// </summary>
public class JellyseerrMediaItem
{
    public int Id { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string? PosterPath { get; set; }

    public string Title { get; set; } = string.Empty;

    public int? Year { get; set; }

    public float? Rating { get; set; }
}

/// <summary>
/// Service for looking up media details from Jellyseerr.
/// </summary>
public class JellyseerrService
{
    private readonly ILogger<JellyseerrService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public JellyseerrService(ILogger<JellyseerrService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient()
    {
        var config = HomeScreenSectionsPlugin.Instance.Configuration;

        if (string.IsNullOrWhiteSpace(config.JellyseerrUrl))
        {
            throw new InvalidOperationException("Jellyseerr URL is not configured.");
        }

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(config.JellyseerrUrl);
        client.DefaultRequestHeaders.Add("X-Api-Key", config.JellyseerrApiKey ?? string.Empty);

        return client;
    }

    public async Task<JellyseerrMediaItem?> LookupMovieByTmdbId(int tmdbId)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/movie/{tmdbId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Jellyseerr lookup failed for TMDB movie {TmdbId}: {Status}", tmdbId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            var dateString = obj.Value<string>("releaseDate") ?? string.Empty;
            int? year = null;

            if (DateTime.TryParse(dateString, out var date))
            {
                year = date.Year;
            }

            return new JellyseerrMediaItem
            {
                Id = tmdbId,
                MediaType = "movie",
                PosterPath = obj.Value<string>("posterPath"),
                Title = obj.Value<string>("title") ?? string.Empty,
                Year = year,
                Rating = obj.Value<float?>("voteAverage")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to lookup movie {TmdbId} in Jellyseerr.", tmdbId);
            return null;
        }
    }

    public async Task<JellyseerrMediaItem?> LookupShowByTmdbId(int tmdbId)
    {
        try
        {
            using var client = CreateClient();
            var response = await client.GetAsync($"/api/v1/tv/{tmdbId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Jellyseerr lookup failed for TMDB show {TmdbId}: {Status}", tmdbId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(json);

            var dateString = obj.Value<string>("firstAirDate") ?? string.Empty;
            int? year = null;

            if (DateTime.TryParse(dateString, out var date))
            {
                year = date.Year;
            }

            return new JellyseerrMediaItem
            {
                Id = tmdbId,
                MediaType = "tv",
                PosterPath = obj.Value<string>("posterPath"),
                Title = obj.Value<string>("name") ?? string.Empty,
                Year = year,
                Rating = obj.Value<float?>("voteAverage")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to lookup show {TmdbId} in Jellyseerr.", tmdbId);
            return null;
        }
    }

    public string GetPosterUrl(string posterPath)
    {
        return $"https://image.tmdb.org/t/p/w600_and_h900_bestv2{posterPath}";
    }
}