using Weikio.PluginFramework.Abstractions;

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
    }
}
