using System;
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
        /// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>.
        /// </summary>
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();

        [Obsolete("Please use TypeFinderOptions. This will be removed in a future release.")]
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
        
        /// <summary>
        /// Gets or sets the <see cref="TypeFinderOptions"/>. 
        /// </summary>
        public TypeFinderOptions TypeFinderOptions { get; set; } = new TypeFinderOptions();

        /// <summary>
        /// Gets or sets the <see cref="ITypeFindingContext"/>.
        /// </summary>
        public ITypeFindingContext TypeFindingContext { get; set; } = null;
    }
}
