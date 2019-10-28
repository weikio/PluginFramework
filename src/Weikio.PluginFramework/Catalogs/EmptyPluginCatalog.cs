using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class EmptyPluginCatalog : IPluginCatalog
    {
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        public bool IsInitialized { get; } = true;

        public Task<List<PluginDefinition>> GetAll()
        {
            return Task.FromResult(new List<PluginDefinition>());
        }

        public Task<PluginDefinition> Get(string name, Version version)
        {
            return null;
        }

        public Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            return null;
        }
    }
}
