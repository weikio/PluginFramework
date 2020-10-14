using System;
using Microsoft.Extensions.Configuration;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace Weikio.PluginFramework.Configuration.Converters
{
    /// <summary>
    /// Converter implementation for the <see cref="AssemblyPluginCatalog"/>.
    /// </summary>
    public class AssemblyCatalogConfigurationCoverter : IConfigurationToCatalogConverter
    {
        ///<inheritdoc/>
        public bool CanConvert(string type)
        {
            return string.Equals(type, "Assembly", StringComparison.InvariantCultureIgnoreCase);
        }

        ///<inheritdoc/>
        public IPluginCatalog Convert(IConfigurationSection section)
        {
            var path = section.GetValue<string>("Path");

            return new AssemblyPluginCatalog(path);
        }
    }
}
