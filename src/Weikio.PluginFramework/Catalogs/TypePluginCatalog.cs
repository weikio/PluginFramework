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

        public TypePluginCatalog(Type pluginType) : this(pluginType, null, null, null)
        {
            
        }
            
        public TypePluginCatalog(Type pluginType, PluginNameOptions nameOptions) : this (pluginType, null, nameOptions, null)
        {
        }

        public TypePluginCatalog(Type pluginType, Action<PluginNameOptions> configure) : this(pluginType, configure, null, null)
        {
        }

        public TypePluginCatalog(Type pluginType, TypePluginCatalogOptions options) : this(pluginType, null, null, options)
        {
        }

        public TypePluginCatalog(Type pluginType, Action<PluginNameOptions> configure, PluginNameOptions nameOptions, TypePluginCatalogOptions options)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }

            _pluginType = pluginType;
            _options = options ?? new TypePluginCatalogOptions();

            if (nameOptions == null)
            {
                nameOptions = new PluginNameOptions();
            }

            if (configure != null)
            {
                configure(nameOptions);
            }

            _options.PluginNameOptions = nameOptions;
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

        public List<Plugin> GetPlugins()
        {
            return new List<Plugin>() { _plugin };
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
