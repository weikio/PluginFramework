using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Weikio.PluginFramework.Configuration.Providers
{
    /// <summary>
    /// Implementation of <see cref="IPluginCatalogConfigurationLoader"/> that
    /// loads a list of <see cref="CatalogConfiguration"/> objects from the <see cref="IConfiguration"/> object.
    /// </summary>
    public class PluginCatalogConfigurationLoader : IPluginCatalogConfigurationLoader
    {
        ///<inheritdoc/>
        public virtual string SectionKey => "PluginFramework";

        ///<inheritdoc/>
        public virtual string CatalogsKey => "Catalogs";

        ///<inheritdoc/>
        public IConfiguration Configuration { get; private set; }

        public PluginCatalogConfigurationLoader(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        ///<inheritdoc/>
        public List<CatalogConfiguration> GetCatalogConfigurations()
        {
            var catalogs = new List<CatalogConfiguration>();

            Configuration.Bind($"{SectionKey}:{CatalogsKey}", catalogs);

            return catalogs;
        }
    }
}
