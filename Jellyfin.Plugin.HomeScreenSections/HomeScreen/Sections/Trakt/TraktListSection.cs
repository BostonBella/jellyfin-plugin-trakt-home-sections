using Jellyfin.Plugin.TraktHomeSections.Configuration;
using Jellyfin.Plugin.TraktHomeSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TraktHomeSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for a configured Trakt list.
    /// </summary>
    public class TraktListSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;
        private readonly TraktListConfig m_listConfig;

        public override string? Section => $"DiscoverTraktList_{m_listConfig.ListId}";

        public override string? DisplayText
        {
            get => !string.IsNullOrWhiteSpace(m_listConfig.DisplayName)
                ? m_listConfig.DisplayName
                : m_listConfig.ListId;
            set { }
        }

        public TraktListSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            TraktListConfig listConfig,
            ILogger<TraktListSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
            m_listConfig = listConfig;
        }

        public override async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = Plugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var items = await m_traktService.GetListItems(m_listConfig.ListId, limit);
            return await ConvertToBaseItemDtos(items, limit);
        }
    }
}