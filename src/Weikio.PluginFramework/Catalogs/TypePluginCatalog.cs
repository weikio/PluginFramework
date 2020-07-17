using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class TypePluginCatalog : IPluginCatalog
    {
        private readonly Type _pluginType;
        private PluginOld _pluginOld;
        private readonly TypePluginCatalogOptions _options;
        private Plugin _plugin;

        public TypePluginCatalog(Type pluginType, PluginNameOptions nameOptions)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }
            
            if (nameOptions == null)
            {
                throw new ArgumentNullException(nameof(nameOptions));
            }
            
            _pluginType = pluginType;
            _options = new TypePluginCatalogOptions()
            {
                PluginNameOptions = nameOptions
            };
        }
        
        public TypePluginCatalog(Type pluginType, Action<PluginNameOptions> configure)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }
            
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }
            
            _pluginType = pluginType;
            
            var nameOptions = new PluginNameOptions();
            configure(nameOptions);
            
            _options = new TypePluginCatalogOptions()
            {
                PluginNameOptions = nameOptions
            };
        }
        
        public TypePluginCatalog(Type pluginType, TypePluginCatalogOptions options = null)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }
            
            _pluginType = pluginType;
            _options = options ?? new TypePluginCatalogOptions();
        }

        public Task Initialize()
        {
            var namingOptions = _options.PluginNameOptions;
            var version = namingOptions.PluginVersionGenerator(namingOptions, _pluginType);
            var pluginName = namingOptions.PluginNameGenerator(namingOptions, _pluginType);
            var description = namingOptions.PluginDescriptionGenerator(namingOptions, _pluginType);
            var productVersion = namingOptions.PluginProductVersionGenerator(namingOptions, _pluginType);

            _pluginOld = new PluginOld(pluginName, version, this, description, productVersion);
            _plugin = new Plugin(_pluginType.Assembly, _pluginType, pluginName, version, this, description, productVersion);
            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public Task<List<PluginOld>> GetPluginsOld()
        {
            var result = new List<PluginOld>() { _pluginOld };

            return Task.FromResult(result);
        }

        public Task<PluginOld> GetPlugin(string name, Version version)
        {
            if (!string.Equals(name, _pluginOld.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _pluginOld.Version)
            {
                return Task.FromResult<PluginOld>(null);
            }

            return Task.FromResult(_pluginOld);
        }

        public Task<Assembly> GetAssembly(PluginOld definition)
        {
            return Task.FromResult(_pluginType.Assembly);
        }

        public bool SupportsUnload { get; }
        public Task Unload()
        {
            throw new NotImplementedException();
        }

        public bool Unloaded { get; }
        public List<Plugin> GetPlugins()
        {
            return new List<Plugin>(){_plugin};
        }

        public Plugin Get(string name, Version version)
        {
            if (!string.Equals(name, _pluginOld.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _pluginOld.Version)
            {
                return null;
            }

            return _plugin;
        }
    }
}
