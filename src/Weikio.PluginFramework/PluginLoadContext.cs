using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Weikio.PluginFramework
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private readonly string _pluginPath;
        private readonly AssemblyDependencyResolver _resolver;
        private readonly PluginLoadContextOptions _options;

        public PluginLoadContext(string pluginPath, PluginLoadContextOptions options = null)
        {
            _pluginPath = pluginPath;
            _resolver = new AssemblyDependencyResolver(pluginPath);
            _options = options ?? new PluginLoadContextOptions();
        }

        public Assembly Load()
        {
            var assemblyName = new AssemblyName(Path.GetFileNameWithoutExtension(_pluginPath));

            var result = LoadFromAssemblyName(assemblyName);

            return result;
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // TODO: Allow configuration for loading assemblies from default context.
            // Cache the results

            if (_options.UseHostApplicationAssemblies)
            {
                try
                {
                    var defaultAssembly = Default.LoadFromAssemblyName(assemblyName);

                    // This assembly is available from default AssemlyLoadContext. Use that instead of this context
                    return null;
                }
                catch
                {
                    // Default-context doesn't have the assembly so try to resolve with this AssemblyLoadContext. 
                }
            }

            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
