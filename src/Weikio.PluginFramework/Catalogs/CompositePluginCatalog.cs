using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Catalogs
{
    public class CompositePluginCatalog : IPluginCatalog
    {
        private readonly List<IPluginCatalog> _catalogs;
        public bool IsInitialized { get; private set; }

        public async Task<List<PluginDefinition>> GetAll()
        {
            var result = new List<PluginDefinition>();

            foreach (var pluginCatalog in _catalogs)
            {
                var pluginsInCatalog = await pluginCatalog.GetAll();
                result.AddRange(pluginsInCatalog);
            }

            return result;
        }

        public async Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.Source == null)
            {
                throw new ArgumentNullException(nameof(definition.Source));
            }

            var result = await definition.Source.GetAssembly(definition);

            return result;
        }

        public CompositePluginCatalog(params IPluginCatalog[] catalogs)
        {
            _catalogs = catalogs.ToList();
        }

        public void AddCatalog(IPluginCatalog catalog)
        {
            _catalogs.Add(catalog);
        }

        public async Task<PluginDefinition> Get(string name, Version version)
        {
            PluginDefinition result = null;

            foreach (var pluginCatalog in _catalogs)
            {
                var plugin = await pluginCatalog.Get(name, version);

                if (plugin == null)
                {
                    continue;
                }

                result = plugin;

                break;
            }

            return result;
        }

        public async Task Initialize()
        {
            if (_catalogs?.Any() != true)
            {
                IsInitialized = true;

                return;
            }

            foreach (var pluginCatalog in _catalogs)
            {
                await pluginCatalog.Initialize();
            }

            IsInitialized = true;
        }
    }
}
