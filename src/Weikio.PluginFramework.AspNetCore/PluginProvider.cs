using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.AspNetCore
{
    public class PluginProvider
    {
        private readonly IEnumerable<IPluginCatalog> _catalogs;
        private readonly IServiceProvider _serviceProvider;

        public PluginProvider(IEnumerable<IPluginCatalog> catalogs, IServiceProvider serviceProvider)
        {
            _catalogs = catalogs;
            _serviceProvider = serviceProvider;
        }

        public List<Plugin> GetByTag(string tag)
        {
            var result = new List<Plugin>();

            foreach (var pluginCatalog in _catalogs)
            {
                var pluginsByTag = pluginCatalog.GetByTag(tag);
                result.AddRange(pluginsByTag);
            }

            return result;
        }

        public List<Plugin> GetPlugins()
        {
            var result = new List<Plugin>();
            foreach (var pluginCatalog in _catalogs)
            {
                result.AddRange(pluginCatalog.GetPlugins());
            }

            return result;
        }
        
        public Plugin Get(string name, Version version)
        {
            foreach (var pluginCatalog in _catalogs)
            {
                var result = pluginCatalog.Get(name, version);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
        
        public List<T> GetTypes<T>() where T : class
        {
            var result = new List<T>();
            var catalogs = _serviceProvider.GetServices<IPluginCatalog>();

            foreach (var catalog in catalogs)
            {
                var plugins = catalog.GetPlugins();

                foreach (var plugin in plugins.Where(x => typeof(T).IsAssignableFrom(x)))
                {
                    var op = plugin.Create<T>(_serviceProvider);

                    result.Add(op);
                }
            }

            return result;
        }
    }
}
