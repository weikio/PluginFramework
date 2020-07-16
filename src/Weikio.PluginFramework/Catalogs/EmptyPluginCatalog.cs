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

        public Task<List<PluginOld>> GetPluginsOld()
        {
            return Task.FromResult(new List<PluginOld>());
        }

        public Task<PluginOld> GetPlugin(string name, Version version)
        {
            return null;
        }

        public Task<Assembly> GetAssembly(PluginOld definition)
        {
            return null;
        }

        public bool SupportsUnload { get; }
        public Task Unload()
        {
            throw new NotImplementedException();
        }

        public bool Unloaded { get; }
        public List<Plugin> GetPlugins()
        {
            throw new NotImplementedException();
        }

        public Plugin Get(string name, Version version)
        {
            throw new NotImplementedException();
        }
    }
}
