using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Weikio.NugetDownloader;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Configuration.Converters;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs.NuGet
{
    public class NugetFeedCatalogConfigurationConverter : IConfigurationToCatalogConverter
    {
        public bool CanConvert(string type)
        {
            return string.Equals(type, "NugetFeed", StringComparison.InvariantCultureIgnoreCase);
        }

        public IPluginCatalog Convert(IConfigurationSection configuration)
        {
            var feedName = configuration.GetValue<string>("Name")
                              ?? throw new ArgumentException("Plugin Framework's NugetFeedCatalog requires a NuGet package name.");

            var feedUrl = configuration.GetValue<string>("Feed")
                           ?? throw new ArgumentException("Plugin Framework's NugetFeedCatalog requires a NuGet feed url.");

            var searchTerm = configuration.GetValue<string>("SearchTerm")
                           ?? throw new ArgumentException("Plugin Framework's NugetFeedCatalog requires a NuGet search term");

            var options = new NugetFeedOptions();
            configuration.Bind("Options", options);

            return new NugetFeedPluginCatalog(new NuGetFeed(feedName, feedUrl)
            {
                Username = options.UserName,
                Password = options.Password
            }, searchTerm, options.IncludePreRelease, options.MaxPackages ?? 128);
        }
    }

    class NugetFeedOptions
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IncludePreRelease { get; set; }

        public int? MaxPackages { get; set; }
    }
}
