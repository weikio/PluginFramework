using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Context
{
    /// <summary>
    /// Defines a Plugin Load Context which allows the loading of plugin specific version's of assemblies.
    /// </summary>
    public class PluginAssemblyLoadContext : AssemblyLoadContext, ITypeFindingContext
    {
        private readonly string _pluginPath;
        private readonly AssemblyDependencyResolver _resolver;
        private readonly PluginLoadContextOptions _options;

        public PluginAssemblyLoadContext(string pluginPath, PluginLoadContextOptions options = null) : base(true)
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
            Log(LogLevel.Debug, "Loading {AssemblyName}", args: assemblyName);

            if (TryUseHostApplicationAssembly(assemblyName))
            {
                try
                {
                    var defaultAssembly = Default.LoadFromAssemblyName(assemblyName);

                    Log(LogLevel.Debug, "Assembly {AssemblyName} is available through host application's AssemblyLoadContext. Use it. ", ex: null, assemblyName);
                    return null;
                }
                catch
                {
                    Log(LogLevel.Debug, "Host application's AssemblyLoadContext doesn't contain {AssemblyName}. Try to resolve it through the plugin's references.", ex: null, assemblyName);
                }
            }

            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

            if (assemblyPath != null)
            {
                Log(LogLevel.Debug, "Loading {AssemblyName} into AssemblyLoadContext from {Path}", ex: null, assemblyName, assemblyPath);

                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        private bool TryUseHostApplicationAssembly(AssemblyName assemblyName)
        {
            Log(LogLevel.Debug, "Determining if {AssemblyName} should be loaded from host application's or from plugin's AssemblyLoadContext",
                args: assemblyName);

            if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
            {
                Log(LogLevel.Debug, "UseHostApplicationAssemblies is set to Never. Try to load assembly from plugin's AssemblyLoadContext", args: assemblyName);

                return false;
            }

            if (_options.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
            {
                Log(LogLevel.Debug, "UseHostApplicationAssemblies is set to Always. Try to load assembly from host application's AssemblyLoadContext",
                    args: assemblyName);

                return true;
            }

            var name = assemblyName.Name;

            var result = _options.HostApplicationAssemblies?.Any(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase)) == true;

            Log(LogLevel.Debug, "UseHostApplicationAssemblies is set to Selected. {AssemblyName} listed in the HostApplicationAssemblies: {Result}", ex: null,
                assemblyName, result);

            return result;
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

        private void Log(LogLevel logLevel, string message, Exception ex = null, params object[] args)
        {
            var logger = GetLogger();

            logger.Log(logLevel, message, ex, args);
        }

        private static string loggerLock = "lock";
        private ILogger<PluginAssemblyLoadContext> _logger;

        private ILogger<PluginAssemblyLoadContext> GetLogger()
        {
            // ReSharper disable once InvertIf
            if (_logger == null)
            {
                lock (loggerLock)
                {
                    if (_logger == null)
                    {
                        if (_options?.LoggerFactory == null)
                        {
                            _logger = NullLogger<PluginAssemblyLoadContext>.Instance;
                        }
                        else
                        {
                            _logger = _options.LoggerFactory();
                        }
                    }
                }
            }

            return _logger;
        }

        public Assembly FindAssembly(string assemblyName)
        {
            return Load(new AssemblyName(assemblyName));
        }

        public Type FindType(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            var assembly = Load(assemblyName);

            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyName);
            }

            var result = assembly.GetType(type.FullName);

            return result;
        }
    }
}
