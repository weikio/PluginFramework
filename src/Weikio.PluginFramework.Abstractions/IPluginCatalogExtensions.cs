using System.Linq;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public static class IPluginCatalogExtensions
    {
        public static Plugin Single(this IPluginCatalog catalog)
        {
            var plugins = catalog.GetPlugins();

            return plugins.Single();
        }
        
        public static Plugin Get(this IPluginCatalog catalog)
        {
            return catalog.Single();
        }
    }
}
