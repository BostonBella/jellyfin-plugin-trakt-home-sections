using Jellyfin.Plugin.HomeScreenSections.Helpers;
using Jellyfin.Plugin.HomeScreenSections.Model.Trakt;
using Jellyfin.Plugin.HomeScreenSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections.Trakt;

/// <summary>
/// Base class for Trakt-backed home screen sections.
/// </summary>
public abstract class TraktSectionBase
{
    private readonly JellyseerrService _jellyseerrService;
    private readonly ImageCacheService _imageCacheService;
    private readonly ILogger _logger;

    protected TraktSectionBase(
        JellyseerrService jellyseerrService,
        ImageCacheService imageCacheService,
        ILogger logger)
    {
        _jellyseerrService = jellyseerrService;
        _imageCacheService = imageCacheService;
        _logger = logger;
    }

    protected async Task<QueryResult<BaseItemDto>> ConvertToBaseItemDtos(
        IEnumerable<TraktListItem> items,
        int limit)
    {
        var results = new List<BaseItemDto>();

        foreach (var item in items)
        {
            if (results.Count >= limit)
            {
                break;
            }

            try
            {
                if (item.Type == "movie" && item.Movie?.Ids.Tmdb != null)
                {
                    var dto = await ConvertMovieToDto(item.Movie);
                    if (dto != null)
                    {
                        results.Add(dto);
                    }
                }
                else if (item.Type == "show" && item.Show?.Ids.Tmdb != null)
                {
                    var dto = await ConvertShowToDto(item.Show);
                    if (dto != null)
                    {
                        results.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert Trakt item to BaseItemDto.");
            }
        }

        return new QueryResult<BaseItemDto>
        {
            Items = results.ToArray(),
            StartIndex = 0,
            TotalRecordCount = results.Count
        };
    }

    protected async Task<QueryResult<BaseItemDto>> ConvertMoviesToBaseItemDtos(
        IEnumerable<TraktMovie> movies,
        int limit)
    {
        var items = movies.Select(movie => new TraktListItem
        {
            Type = "movie",
            Movie = movie
        });

        return await ConvertToBaseItemDtos(items, limit);
    }

    protected async Task<QueryResult<BaseItemDto>> ConvertShowsToBaseItemDtos(
        IEnumerable<TraktShow> shows,
        int limit)
    {
        var items = shows.Select(show => new TraktListItem
        {
            Type = "show",
            Show = show
        });

        return await ConvertToBaseItemDtos(items, limit);
    }

    private async Task<BaseItemDto?> ConvertMovieToDto(TraktMovie movie)
    {
        if (movie.Ids.Tmdb == null)
        {
            return null;
        }

        var tmdbId = movie.Ids.Tmdb.Value;
        var jellyseerrItem = await _jellyseerrService.LookupMovieByTmdbId(tmdbId);

        if (jellyseerrItem == null)
        {
            return null;
        }

        var posterUrl = !string.IsNullOrWhiteSpace(jellyseerrItem.PosterPath)
            ? GetCachedImageUrl(_jellyseerrService.GetPosterUrl(jellyseerrItem.PosterPath))
            : string.Empty;

        return new BaseItemDto
        {
            Name = jellyseerrItem.Title,
            SourceType = "movie",
            CommunityRating = jellyseerrItem.Rating,
            PremiereDate = jellyseerrItem.Year.HasValue
                ? new DateTime(jellyseerrItem.Year.Value, 1, 1)
                : null,
            ProviderIds = BuildProviderIds(tmdbId, posterUrl)
        };
    }

    private async Task<BaseItemDto?> ConvertShowToDto(TraktShow show)
    {
        if (show.Ids.Tmdb == null)
        {
            return null;
        }

        var tmdbId = show.Ids.Tmdb.Value;
        var jellyseerrItem = await _jellyseerrService.LookupShowByTmdbId(tmdbId);

        if (jellyseerrItem == null)
        {
            return null;
        }

        var posterUrl = !string.IsNullOrWhiteSpace(jellyseerrItem.PosterPath)
            ? GetCachedImageUrl(_jellyseerrService.GetPosterUrl(jellyseerrItem.PosterPath))
            : string.Empty;

        return new BaseItemDto
        {
            Name = jellyseerrItem.Title,
            SourceType = "tv",
            CommunityRating = jellyseerrItem.Rating,
            PremiereDate = jellyseerrItem.Year.HasValue
                ? new DateTime(jellyseerrItem.Year.Value, 1, 1)
                : null,
            ProviderIds = BuildProviderIds(tmdbId, posterUrl)
        };
    }

    private static Dictionary<string, string> BuildProviderIds(int tmdbId, string posterUrl)
    {
        var config = HomeScreenSectionsPlugin.Instance.Configuration;

        var jellyseerrDisplayUrl = !string.IsNullOrWhiteSpace(config.JellyseerrExternalUrl)
            ? config.JellyseerrExternalUrl
            : config.JellyseerrUrl;

        return new Dictionary<string, string>
        {
            { "JellyseerrRoot", jellyseerrDisplayUrl ?? string.Empty },
            { "Jellyseerr", tmdbId.ToString() },
            { "JellyseerrPoster", posterUrl }
        };
    }

    private string GetCachedImageUrl(string sourceUrl)
    {
        return ImageCacheHelper.GetCachedImageUrl(_imageCacheService, sourceUrl, _logger);
    }
}