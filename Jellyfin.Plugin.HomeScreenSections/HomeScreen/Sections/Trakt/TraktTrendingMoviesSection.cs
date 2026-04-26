using Jellyfin.Plugin.TraktHomeSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TraktHomeSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for Trakt trending movies.
    /// </summary>
    public class TraktTrendingMoviesSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public override string? Section => "DiscoverTraktTrendingMovies";

        public override string? DisplayText
        {
            get => Plugin.Instance.Configuration.TraktTrendingMoviesName;
            set { }
        }

        public TraktTrendingMoviesSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktTrendingMoviesSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public override async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = Plugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var movies = await m_traktService.GetTrendingMovies(limit);
            return await ConvertMoviesToBaseItemDtos(movies, limit);
        }
    }
}