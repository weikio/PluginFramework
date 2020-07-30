using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    /// <summary>
    /// Empty Plugin catalog. Doesn't contain anything, is automatically initialized when created. 
    /// </summary>
    public class EmptyPluginCatalog : IPluginCatalog
    {
        /// <inheritdoc />
        public Task Initialize()
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool IsInitialized { get; } = true;

        /// <inheritdoc />
        public List<Plugin> GetPlugins()
        {
            return new List<Plugin>();
        }

        /// <inheritdoc />
        public Plugin Get(string name, Version version)
        {
            return null;
        }
    }
}
