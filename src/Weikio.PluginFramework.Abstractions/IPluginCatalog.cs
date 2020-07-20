using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public interface IPluginCatalog
    {
        /// <summary>
        /// Initializes the catalog
        /// </summary>
        Task Initialize();
        
        /// <summary>
        /// Gets if the catalog is initialized
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// Gets all the plugins
        /// </summary>
        /// <returns>List of <see cref="Plugin"/></returns>
        List<Plugin> GetPlugins();
        
        /// <summary>
        /// Gets a single plugin based on its name and version
        /// </summary>
        /// <returns>The <see cref="Plugin"/></returns>
        Plugin Get(string name, Version version);
    }
}
