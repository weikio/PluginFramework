using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Catalogs
{
    public class NugetFeedPluginCatalog : IPluginCatalog
    {
        public Task Initialize()
        {
            throw new NotImplementedException();
        }

        public bool IsInitialized { get; private set; }

        public Task<List<PluginDefinition>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<PluginDefinition> Get(string name, Version version)
        {
            throw new NotImplementedException();
        }

        public Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            throw new NotImplementedException();
        }
    }
}
