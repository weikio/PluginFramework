using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Weikio.PluginFramework.Context
{
    /// <summary>
    /// Options for PluginLoadContext
    /// </summary>
    public class PluginLoadContextOptions
    {
       
        /// <summary>
        /// Gets or sets if the plugin should by default to use the assemblies referenced by the plugin or by the host application. Useful in situations where it is important that the host application
        /// and the plugin use the same version of the assembly, even if they reference different versions.
        /// </summary>
        public UseHostApplicationAssembliesEnum UseHostApplicationAssemblies { get; set; } = Defaults.UseHostApplicationAssemblies;

        /// <summary>
        /// Gets the assemblies which the plugin should use if UseHostApplicationAssemblies is set to Selected. These assemblies are used
        /// even if the plugin itself references an another version of the same assembly.
        /// </summary>
        public List<AssemblyName> HostApplicationAssemblies { get; set; } = Defaults.HostApplicationAssemblies;

        /// <summary>
        /// Sets the function which is used to create the logger for PluginLoadContextOptions
        /// </summary>
        public Func<ILogger<PluginAssemblyLoadContext>> LoggerFactory { get; set; } = Defaults.LoggerFactory;
        
        
        public static class Defaults
        {
            /// <summary>
            /// Gets or sets if the plugin should by default to use the assemblies referenced by the plugin or by the host application. Default = Always. Useful in situations where it is important that the host application
            /// and the plugin use the same version of the assembly, even if they reference different versions. 
            /// </summary>
            public static UseHostApplicationAssembliesEnum UseHostApplicationAssemblies { get; set; } = UseHostApplicationAssembliesEnum.Always;

            /// <summary>
            /// Gets the assemblies which the plugin should use if UseHostApplicationAssemblies is set to Selected. These assemblies are used
            /// even if the plugin itself references an another version of the same assembly.
            /// </summary>
            public static List<AssemblyName> HostApplicationAssemblies { get; set; } = new List<AssemblyName>();

            /// <summary>
            /// Sets the function which is used to create the logger for PluginLoadContextOptions
            /// </summary>
            public static Func<ILogger<PluginAssemblyLoadContext>> LoggerFactory { get; set; } = () => NullLogger<PluginAssemblyLoadContext>.Instance;
        }
    }

}
