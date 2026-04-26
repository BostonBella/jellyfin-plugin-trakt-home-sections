using Jellyfin.Plugin.TraktHomeSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TraktHomeSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for the Trakt watchlist.
    /// </summary>
    public class TraktWatchlistSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public override string? Section => "DiscoverTraktWatchlist";

        public override string? DisplayText
        {
            get => Plugin.Instance.Configuration.TraktWatchlistName;
            set { }
        }

        public TraktWatchlistSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktWatchlistSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public override async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = Plugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var items = await m_traktService.GetWatchlist(limit);
            return await ConvertToBaseItemDtos(items, limit);
        }
    }
}