using System;
using NuGet.Common;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.NugetDownloader;
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

        /// <summary>
        /// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
        /// </summary>
        public PluginNameOptions PluginNameOptions { get; set; } = Defaults.PluginNameOptions;

        /// <summary>
        /// Gets or sets if system feeds should be used as secondary feeds for finding packages when feed url is defined.
        /// </summary>
        public bool IncludeSystemFeedsAsSecondary { get; set; } = false;

        /// <summary>
        /// Gets or sets the target platform. If not set, EntryAssembly's target framework is used.
        /// </summary>
        public string TargetFramework { get; set; } = Defaults.TargetFramework;
        
        public static class Defaults
        {
            /// <summary>
            /// Gets or sets the default function which is used to create the logger for PluginLoadContextOptions
            /// </summary>
            public static Func<ILogger> LoggerFactory { get; set; } = () => new ConsoleLogger();
            
            /// <summary>
            /// Gets or sets the default of how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
            /// </summary>
            public static PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
            
            /// <summary>
            /// Gets or sets the default target platform. If not set, EntryAssembly's target framework is used.
            /// </summary>
            public static string TargetFramework { get; set; } = string.Empty;
        }
    }
}
