using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.TraktHomeSections.JellyfinVersionSpecific
{
    public static class ExtensionsHelper
    {
        public static bool IsPlayedVersionSpecific(this BaseItem item, User user)
        {
            return item.IsPlayed(user);
        }
    }
}