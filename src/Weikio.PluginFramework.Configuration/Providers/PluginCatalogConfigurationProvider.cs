using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Weikio.PluginFramework.Configuration.Providers
{
    /// <summary>
    /// Implementation of <see cref="IPluginCatalogConfigurationProvider"/> that
    /// loads a list of <see cref="CatalogConfiguration"/> objects from the <see cref="IConfiguration"/> object.
    /// </summary>
    public class PluginCatalogConfigurationProvider : IPluginCatalogConfigurationProvider
    {
        ///<inheritdoc/>
        public string SectionKey => "PluginFramework";

        ///<inheritdoc/>
        public string CatalogsKey => "Catalogs";

        ///<inheritdoc/>
        public List<CatalogConfiguration> GetCatalogConfigurations(IConfiguration configuration)
        {
            var catalogs = new List<CatalogConfiguration>();

            configuration.Bind($"{SectionKey}:{CatalogsKey}", catalogs);

            return catalogs;
        }
    }
}
