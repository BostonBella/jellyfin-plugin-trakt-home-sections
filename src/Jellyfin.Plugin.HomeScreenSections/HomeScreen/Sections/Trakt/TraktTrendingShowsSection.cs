using Jellyfin.Plugin.HomeScreenSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for Trakt trending shows.
    /// </summary>
    public class TraktTrendingShowsSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public TraktTrendingShowsSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktTrendingShowsSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var shows = await m_traktService.GetTrendingShows(limit);
            return await ConvertShowsToBaseItemDtos(shows, limit);
        }
    }
}