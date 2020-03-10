using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public interface IPluginCatalog
    {
        Task Initialize();
        bool IsInitialized { get; }
        Task<List<PluginDefinition>> GetAll();
        Task<PluginDefinition> Get(string name, Version version);
        Task<Assembly> GetAssembly(PluginDefinition definition);
        bool SupportsUnload { get; }
        Task Unload();
        bool Unloaded { get; }
    }
}
