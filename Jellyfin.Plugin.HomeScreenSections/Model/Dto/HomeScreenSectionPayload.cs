namespace Jellyfin.Plugin.TraktHomeSections.Model.Dto
{
    public class HomeScreenSectionPayload
    {
        public Guid UserId { get; set; }

        public string? AdditionalData { get; set; }
    }
}
