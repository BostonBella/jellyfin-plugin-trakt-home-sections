using Jellyfin.Plugin.HomeScreenSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for Trakt movie recommendations.
    /// </summary>
    public class TraktMovieRecommendationsSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public TraktMovieRecommendationsSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktMovieRecommendationsSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var movies = await m_traktService.GetMovieRecommendations(limit);
            return await ConvertMoviesToBaseItemDtos(movies, limit);
        }
    }
}