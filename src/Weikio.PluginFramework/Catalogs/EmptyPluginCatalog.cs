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

        public List<Plugin> GetPlugins()
        {
            return new List<Plugin>();
        }

        public Plugin Get(string name, Version version)
        {
            return null;
        }
    }
}
