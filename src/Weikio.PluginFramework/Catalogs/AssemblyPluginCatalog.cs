using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalog : IPluginCatalog
    {
        private readonly string _assemblyPath;
        private readonly Assembly _assembly;
        private PluginDefinition _pluginDefinition;
        private readonly AssemblyPluginCatalogOptions _options;

        public AssemblyPluginCatalog(string assemblyPath, AssemblyPluginCatalogOptions options = null)
        {
            _options = options ?? new AssemblyPluginCatalogOptions();
            
            var loadContext = new PluginLoadContext(assemblyPath, _options.PluginLoadContextOptions);
            _assembly = loadContext.Load();

            _assemblyPath = assemblyPath;
        }

        public AssemblyPluginCatalog(Assembly assembly, AssemblyPluginCatalogOptions options = null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
            _options = options ?? new AssemblyPluginCatalogOptions();
        }

        public Task Initialize()
        {
            _pluginDefinition = AssemblyToPluginDefinitionConverter.Convert(_assembly, this);

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
            return Task.FromResult(_assembly);
        }
    }
}
