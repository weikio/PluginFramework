using System.Collections.Generic;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    /// <summary>
    /// Options for configuring how the <see cref="TypePluginCatalogOptions"/> works.
    /// </summary>
    public class TypePluginCatalogOptions
    {
        /// <summary>
        /// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>
        /// </summary>
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
        
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();

        public ITypeFindingContext TypeFindingContext { get; set; } = null;
    }
}
