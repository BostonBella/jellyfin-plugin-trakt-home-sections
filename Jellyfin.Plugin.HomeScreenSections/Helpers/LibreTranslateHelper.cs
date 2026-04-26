using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.TraktHomeSections.Helpers;

public static class LibreTranslateHelper
{
    public static async Task<string?> TranslateAsync(string text, string srcLanguage, string destLanguage)
    {
        if (Plugin.Instance.Configuration.LibreTranslateUrl != null)
        {
            try
            {
                JObject jsonPayload = new JObject();
                jsonPayload["q"] = text;
                jsonPayload["source"] = srcLanguage;
                jsonPayload["target"] = destLanguage;
                jsonPayload["api_key"] = Plugin.Instance.Configuration.LibreTranslateApiKey;

                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync(
                    $"{Plugin.Instance.Configuration.LibreTranslateUrl}/translate",
                    new StringContent(jsonPayload.ToString(Formatting.None),
                        MediaTypeHeaderValue.Parse("application/json")));

                JObject responseObj = JObject.Parse(await response.Content.ReadAsStringAsync());

                return responseObj.Value<string>("translatedText");
            }
            catch
            {
                // ignored
            }
        }

        return null;
    }
}