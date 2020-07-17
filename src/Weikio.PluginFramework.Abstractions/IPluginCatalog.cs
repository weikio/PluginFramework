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
        List<Plugin> GetPlugins();
        Plugin Get(string name, Version version);
    }
}
