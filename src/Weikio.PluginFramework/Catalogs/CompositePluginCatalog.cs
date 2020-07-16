using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class CompositePluginCatalog : IPluginCatalog
    {
        private readonly List<IPluginCatalog> _catalogs;
        public bool IsInitialized { get; private set; }

        public async Task<List<PluginOld>> GetPluginsOld()
        {
            var result = new List<PluginOld>();

            foreach (var pluginCatalog in _catalogs)
            {
                var pluginsInCatalog = await pluginCatalog.GetPluginsOld();
                result.AddRange(pluginsInCatalog);
            }

            return result;
        }

        public async Task<Assembly> GetAssembly(PluginOld definition)
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

        public bool SupportsUnload { get; }
        public Task Unload()
        {
            throw new NotImplementedException();
        }

        public bool Unloaded { get; }
        public List<Plugin> GetPlugins()
        {
            var result = new List<Plugin>();

            foreach (var pluginCatalog in _catalogs)
            {
                var pluginsInCatalog = pluginCatalog.GetPlugins();
                result.AddRange(pluginsInCatalog);
            }

            return result;
        }

        public Plugin Get(string name, Version version)
        {
            throw new NotImplementedException();
        }

        public CompositePluginCatalog(params IPluginCatalog[] catalogs)
        {
            _catalogs = catalogs.ToList();
        }

        public void AddCatalog(IPluginCatalog catalog)
        {
            _catalogs.Add(catalog);
        }

        public async Task<PluginOld> GetPlugin(string name, Version version)
        {
            PluginOld result = null;

            foreach (var pluginCatalog in _catalogs)
            {
                var plugin = await pluginCatalog.GetPlugin(name, version);

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
