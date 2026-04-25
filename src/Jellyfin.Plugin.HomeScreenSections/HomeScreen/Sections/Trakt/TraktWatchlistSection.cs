using Jellyfin.Plugin.HomeScreenSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for the Trakt watchlist.
    /// </summary>
    public class TraktWatchlistSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public TraktWatchlistSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktWatchlistSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var items = await m_traktService.GetWatchlist(limit);
            return await ConvertToBaseItemDtos(items, limit);
        }
    }
}