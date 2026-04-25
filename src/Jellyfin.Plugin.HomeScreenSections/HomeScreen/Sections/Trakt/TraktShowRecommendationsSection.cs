using Jellyfin.Plugin.HomeScreenSections.Services;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.HomeScreenSections.HomeScreen.Sections.Trakt
{
    /// <summary>
    /// Home screen section result handler for Trakt show recommendations.
    /// </summary>
    public class TraktShowRecommendationsSection : TraktSectionBase
    {
        private readonly TraktService m_traktService;

        public override string? Section => "TraktShowRecommendations";

        public override string? DisplayText
        {
            get => HomeScreenSectionsPlugin.Instance.Configuration.TraktShowRecommendationsName;
            set { }
        }

        public TraktShowRecommendationsSection(
            TraktService traktService,
            JellyseerrService jellyseerrService,
            ImageCacheService imageCacheService,
            ILogger<TraktShowRecommendationsSection> logger)
            : base(jellyseerrService, imageCacheService, logger)
        {
            m_traktService = traktService;
        }

        public override async Task<QueryResult<BaseItemDto>> GetResults()
        {
            var config = HomeScreenSectionsPlugin.Instance.Configuration;
            var limit = config.TraktItemLimit;

            var shows = await m_traktService.GetShowRecommendations(limit);
            return await ConvertShowsToBaseItemDtos(shows, limit);
        }
    }
}