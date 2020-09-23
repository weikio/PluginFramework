using Microsoft.Extensions.Configuration;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Configuration.Converters
{
    /// <summary>
    /// Interface that specifies the methods a ConfigurationConverter needs.
    /// </summary>
    public interface IConfigurationToCatalogConverter
    {
        /// <summary>
        /// Determines if the converter can convert the provided type.
        /// True if it can, false otherwise.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>True if the type can be converted.</returns>
        bool CanConvert(string type);

        /// <summary>
        /// Convert a Catalog Configuration to it's equivalent <see cref="IPluginCatalog"/> object.
        /// </summary>
        /// <param name="section">The section that contains the catalog configuration.</param>
        /// <returns>An equivalent <see cref="IPluginCatalog"/> object.</returns>
        IPluginCatalog Convert(IConfigurationSection section);
    }
}
