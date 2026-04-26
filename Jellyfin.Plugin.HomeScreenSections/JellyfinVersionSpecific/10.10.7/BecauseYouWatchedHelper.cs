using Jellyfin.Plugin.TraktHomeSections.Model.Dto;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using Microsoft.AspNetCore.Identity;

namespace Jellyfin.Plugin.TraktHomeSections.JellyfinVersionSpecific
{
    public static class BecauseYouWatchedHelper
    {
        public static InternalItemsQuery ApplySimilarSettings(this InternalItemsQuery query, BaseItem item)
        {
            query.IsMovie = true;
            query.EnableGroupByMetadataKey = true;
            query.SimilarTo = item;

            return query;
        }
    }
}