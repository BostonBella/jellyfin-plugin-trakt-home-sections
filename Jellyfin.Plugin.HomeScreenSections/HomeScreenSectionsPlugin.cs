using System.Reflection;
using System.Runtime.Loader;
using Jellyfin.Plugin.TraktHomeSections.Configuration;
using Jellyfin.Plugin.TraktHomeSections.Library;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jellyfin.Plugin.TraktHomeSections
{
    public class Plugin : BasePlugin<PluginConfiguration>, IPlugin, IHasPluginConfiguration, IHasWebPages
    {
        internal IServerConfigurationManager ServerConfigurationManager { get; private set; }
        
        public override Guid Id => Guid.Parse("dad4eb58-9344-45a5-8cc1-7bd0d7eb9da2");

        public override string Name => "Trakt Home Sections";

        public static Plugin Instance { get; private set; } = null!;
        
        internal IServiceProvider ServiceProvider { get; set; }
    
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IServerConfigurationManager serverConfigurationManager, IServiceProvider serviceProvider, IHomeScreenManager homeScreenManager, ITranslationManager translationManager) : base(applicationPaths, xmlSerializer)
        {
            int pluginPageConfigVersion = 1;
            Instance = this;
            
            ServerConfigurationManager = serverConfigurationManager;
            ServiceProvider = serviceProvider;
            
            homeScreenManager.RegisterBuiltInResultsDelegates();
        
            string homeScreenSectionsConfigDir = Path.Combine(applicationPaths.PluginConfigurationsPath, "Jellyfin.Plugin.TraktHomeSections");
            if (!Directory.Exists(homeScreenSectionsConfigDir))
            {
                Directory.CreateDirectory(homeScreenSectionsConfigDir);
            }
        
            translationManager.Initialize();
            
            string pluginPagesConfig = Path.Combine(applicationPaths.PluginConfigurationsPath, "Jellyfin.Plugin.PluginPages", "config.json");
        
            JObject config = new JObject();
            if (!File.Exists(pluginPagesConfig))
            {
                FileInfo info = new FileInfo(pluginPagesConfig);
                info.Directory?.Create();
            }
            else
            {
                config = JObject.Parse(File.ReadAllText(pluginPagesConfig));
            }

            if (!config.ContainsKey("pages"))
            {
                config.Add("pages", new JArray());
            }

            JObject? hssPageConfig = config.Value<JArray>("pages")!.FirstOrDefault(x =>
                x.Value<string>("Id") == typeof(Plugin).Namespace) as JObject;

            if (hssPageConfig != null)
            {
                if ((hssPageConfig.Value<int?>("Version") ?? 0) < pluginPageConfigVersion)
                {
                    config.Value<JArray>("pages")!.Remove(hssPageConfig);
                }
            }
            
            if (!config.Value<JArray>("pages")!.Any(x => x.Value<string>("Id") == typeof(Plugin).Namespace))
            {
                Assembly? pluginPagesAssembly = AssemblyLoadContext.All.SelectMany(x => x.Assemblies).FirstOrDefault(x => x.FullName?.Contains("Jellyfin.Plugin.PluginPages") ?? false);
                
                Version earliestVersionWithSubUrls = new Version("2.4.1.0");
                bool supportsSubUrls = pluginPagesAssembly != null && pluginPagesAssembly.GetName().Version >= earliestVersionWithSubUrls;
                
                string rootUrl = ServerConfigurationManager.GetNetworkConfiguration().BaseUrl.TrimStart('/').Trim();
                if (!string.IsNullOrEmpty(rootUrl))
                {
                    rootUrl = $"/{rootUrl}";
                }
                
                config.Value<JArray>("pages")!.Add(new JObject
                {
                    { "Id", typeof(Plugin).Namespace },
                    { "Url", $"{(supportsSubUrls ? "" : rootUrl)}/ModularHomeViews/settings" },
                    { "DisplayText", "Modular Home" },
                    { "Icon", "ballot" },
                    { "Version", pluginPageConfigVersion }
                });
        
                File.WriteAllText(pluginPagesConfig, config.ToString(Formatting.Indented));
            }
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            string? prefix = GetType().Namespace;

            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = $"{prefix}.Configuration.config.html",
                EnableInMainMenu = true
            };
        }

        /// <summary>
        /// Get the views that the plugin serves.
        /// </summary>
        /// <returns>Array of <see cref="PluginPageInfo"/>.</returns>
        public IEnumerable<PluginPageInfo> GetViews()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "settings",
                    EmbeddedResourcePath = $"{GetType().Namespace}.Config.settings.html"
                }
            };
        }

        /// <summary>
        /// Override UpdateConfiguration to preserve cache bust counter and config version.
        /// </summary>
        /// <param name="configuration">The new configuration to save</param>
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            if (configuration is PluginConfiguration pluginConfig)
            {
                var currentConfig = base.Configuration;

                // Handle cache busting when developer mode is turned ON
                if (!currentConfig.DeveloperMode && pluginConfig.DeveloperMode)
                {
                    pluginConfig.CacheBustCounter = currentConfig.CacheBustCounter + 1;
                }
                else
                {
                    pluginConfig.CacheBustCounter = currentConfig.CacheBustCounter;
                }

                // Preserve Trakt authorization tokens during normal UI config saves.
                // The admin config page should not expose tokens in the browser, so saves from the UI
                // may not include them. Without this, saving any plugin setting can wipe the linked account.
                if (string.IsNullOrWhiteSpace(pluginConfig.TraktAccessToken))
                {
                    pluginConfig.TraktAccessToken = currentConfig.TraktAccessToken;
                }

                if (string.IsNullOrWhiteSpace(pluginConfig.TraktRefreshToken))
                {
                    pluginConfig.TraktRefreshToken = currentConfig.TraktRefreshToken;
                }

                if (pluginConfig.TraktTokenExpiration == DateTime.MinValue)
                {
                    pluginConfig.TraktTokenExpiration = currentConfig.TraktTokenExpiration;
                }
            }

            base.UpdateConfiguration(configuration);
        }

        /// <summary>
        /// Increment the cache bust counter and save configuration.
        /// </summary>
        public void BustCache()
        {
            var config = base.Configuration;
            config.CacheBustCounter++;
            base.UpdateConfiguration(config);
        }

        /// <summary>
        /// Get the current plugin version.
        /// </summary>
        public string GetCurrentPluginVersion()
        {
            return base.Version?.ToString() ?? "0.0.0";
        }
    }
}