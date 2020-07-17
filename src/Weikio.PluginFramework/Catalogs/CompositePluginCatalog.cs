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
            foreach (var pluginCatalog in _catalogs)
            {
                var plugin = pluginCatalog.Get(name, version);

                if (plugin == null)
                {
                    continue;
                }

                return plugin;
            }

            return null;
        }

        public CompositePluginCatalog(params IPluginCatalog[] catalogs)
        {
            _catalogs = catalogs.ToList();
        }

        public void AddCatalog(IPluginCatalog catalog)
        {
            _catalogs.Add(catalog);
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
