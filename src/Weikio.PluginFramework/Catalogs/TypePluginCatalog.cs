using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class TypePluginCatalog : IPluginCatalog
    {
        private readonly Type _pluginType;
        private PluginDefinition _pluginDefinition;
        private readonly TypePluginCatalogOptions _options;

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

            _pluginDefinition = new PluginDefinition(pluginName, version, this, description, productVersion);

            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public Task<List<PluginDefinition>> GetAll()
        {
            var result = new List<PluginDefinition>() { _pluginDefinition };

            return Task.FromResult(result);
        }

        public Task<PluginDefinition> Get(string name, Version version)
        {
            if (!string.Equals(name, _pluginDefinition.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _pluginDefinition.Version)
            {
                return Task.FromResult<PluginDefinition>(null);
            }

            return Task.FromResult(_pluginDefinition);
        }

        public Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            return Task.FromResult(_pluginType.Assembly);
        }

        public bool SupportsUnload { get; }
        public Task Unload()
        {
            throw new NotImplementedException();
        }

        public bool Unloaded { get; }
    }
}
