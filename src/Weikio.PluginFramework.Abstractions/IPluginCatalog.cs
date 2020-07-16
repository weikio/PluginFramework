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
        Task<List<PluginOld>> GetPluginsOld();
        Task<PluginOld> GetPlugin(string name, Version version);
        Task<Assembly> GetAssembly(PluginOld definition);
        bool SupportsUnload { get; }
        Task Unload();
        bool Unloaded { get; }
        List<Plugin> GetPlugins();
        Plugin Get(string name, Version version);
    }
}
