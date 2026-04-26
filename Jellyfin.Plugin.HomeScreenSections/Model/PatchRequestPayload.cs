using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TraktHomeSections.Model
{
    public class PatchRequestPayload
    {
        [JsonPropertyName("contents")]
        public string? Contents { get; set; }
    }
}