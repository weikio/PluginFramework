using System;
using System.Collections.Generic;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalogOptions
    {
        /// <summary>
        /// Gets or sets the <see cref="PluginLoadContextOptions"/>.
        /// </summary>
        public PluginLoadContextOptions PluginLoadContextOptions = new PluginLoadContextOptions();
        
        [Obsolete("Please use TypeFinderOptions. This will be removed in a future release.")]
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();

        /// <summary>
        /// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
        /// </summary>
        public PluginNameOptions PluginNameOptions { get; set; } = Defaults.PluginNameOptions;
        
        /// <summary>
        /// Gets or sets the <see cref="TypeFinderOptions"/>. 
        /// </summary>
        public TypeFinderOptions TypeFinderOptions { get; set; } = new TypeFinderOptions();

        public static class Defaults
        {
            public static PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
        }
    }
}
