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

        public TypePluginCatalog(Type pluginType, TypePluginCatalogOptions options = null)
        {
            _pluginType = pluginType;
            _options = options ?? new TypePluginCatalogOptions();
        }

        public Task Initialize()
        {
            var version = _options.PluginVersionGenerator(_options, _pluginType);
            var pluginName = _options.PluginNameGenerator(_options, _pluginType);
            var description = _options.PluginDescriptionGenerator(_options, _pluginType);
            var productVersion = _options.PluginProductVersionGenerator(_options, _pluginType);

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
