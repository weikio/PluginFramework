using System;
using NuGet.Common;
using Weikio.PluginFramework.Catalogs.NuGet.PackageManagement;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs.NuGet
{
    public class NugetPluginCatalogOptions
    {
        /// <summary>
        /// Gets or sets the function which is used to create the logger for Nuget activities
        /// </summary>
        public Func <ILogger> LoggerFactory { get; set; } = Defaults.LoggerFactory;
        
        /// <summary>
        /// Gets or sets the <see cref="TypeFinderOptions"/>. 
        /// </summary>
        public TypeFinderOptions TypeFinderOptions { get; set; } = new TypeFinderOptions();

        public static class Defaults
        {
            /// <summary>
            /// Gets or sets the default function which is used to create the logger for PluginLoadContextOptions
            /// </summary>
            public static Func<ILogger> LoggerFactory { get; set; } = () => new ConsoleLogger();
        }
    }
}
