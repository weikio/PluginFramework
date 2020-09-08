using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public static class IPluginCatalogExtensions
    {
        /// <summary>
        /// Gets the only plugin inside the catalog. Throws if there is none or multiple.
        /// </summary>
        /// <param name="catalog">The catalog from which the plugin is retrieved.</param>
        /// <returns>The plugin</returns>
        public static Plugin Single(this IPluginCatalog catalog)
        {
            var plugins = catalog.GetPlugins();

            return plugins.Single();
        }
        
        /// <summary>
        /// Gets the only plugin inside the catalog. Throws if there is none or multiple.
        /// </summary>
        /// <param name="catalog">The catalog from which the plugin is retrieved.</param>
        /// <returns>The plugin</returns>
        public static Plugin Get(this IPluginCatalog catalog)
        {
            return catalog.Single();
        }

        /// <summary>
        /// Gets the plugins by tag.
        /// </summary>
        /// <param name="catalog">The catalog from which the plugin is retrieved.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The plugin</returns>
        public static List<Plugin> GetByTag(this IPluginCatalog catalog, string tag)
        {
            return catalog.GetPlugins().Where(x => x.Tags.Contains(tag)).ToList();
        }
    }
}
