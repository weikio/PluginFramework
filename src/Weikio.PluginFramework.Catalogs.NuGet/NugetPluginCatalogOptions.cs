using System;
using NuGet.Common;
using Weikio.PluginFramework.Catalogs.NuGet.PackageManagement;

namespace Weikio.PluginFramework.Catalogs
{
    public class NugetPluginCatalogOptions
    {
        /// <summary>
        /// Gets or sets the function which is used to create the logger for Nuget activities
        /// </summary>
        public Func <ILogger> LoggerFactory { get; set; } = Defaults.LoggerFactory;

        public static class Defaults
        {
            /// <summary>
            /// Gets or sets the default function which is used to create the logger for PluginLoadContextOptions
            /// </summary>
            public static Func<ILogger> LoggerFactory { get; set; } = () => new ConsoleLogger();
        }
    }
}
