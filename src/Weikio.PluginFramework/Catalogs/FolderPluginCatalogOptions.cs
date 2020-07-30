using System.Collections.Generic;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    /// <summary>
    /// Options for configuring how the <see cref="FolderPluginCatalog"/> works.
    /// </summary>
    public class FolderPluginCatalogOptions
    {
        /// <summary>
        /// Gets or sets if subfolders should be included. Defaults to true.
        /// </summary>
        public bool IncludeSubfolders { get; set; } = true;

        /// <summary>
        /// Gets or sets the search patterns when locating plugins. By default only located dll-files.
        /// </summary>
        public List<string> SearchPatterns { get; set; } = new List<string>() { "*.dll" };

        /// <summary>
        /// Gets or sets the <see cref="PluginLoadContextOptions"/>.
        /// </summary>
        public PluginLoadContextOptions PluginLoadContextOptions { get; set; } = new PluginLoadContextOptions();

        /// <summary>
        /// Gets or sets a single type finder criteria
        /// </summary>
        public TypeFinderCriteria TypeFinderCriteria { get; set; }

        /// <summary>
        /// Gets or sets a collection of type finder criteria. The key is used to "tag" found plugins.
        /// </summary>
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias { get; set; } = new Dictionary<string, TypeFinderCriteria>();

        /// <summary>
        /// Gets or sets how the plugin names and version should be defined. <seealso cref="PluginNameOptions"/>
        /// </summary>
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
    }
}
