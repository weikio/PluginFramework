using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalog : IPluginCatalog
    {
        private readonly string _assemblyPath;
        private Assembly _assembly;
        private PluginOld _pluginOld;
        private readonly AssemblyPluginCatalogOptions _options;
        private PluginAssemblyLoadContext _pluginAssemblyLoadContext;

        public AssemblyPluginCatalog(string assemblyPath, TypeFinderCriteria criteria = null, AssemblyPluginCatalogOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath));
            }

            _assemblyPath = assemblyPath;
            _options = options ?? new AssemblyPluginCatalogOptions();

            if (criteria != null)
            {
                _options.TypeFinderCriterias.Add(string.Empty, criteria);
            }
        }

        public AssemblyPluginCatalog(string assemblyPath, Predicate<Type> filter = null, AssemblyPluginCatalogOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath));
            }

            _assemblyPath = assemblyPath;
            _options = options ?? new AssemblyPluginCatalogOptions();

            if (filter != null)
            {
                var criteria = new TypeFinderCriteria();

                criteria.Query = (context, type) => filter(type);

                _options.TypeFinderCriterias.Add(string.Empty, criteria);
            }
        }

        public AssemblyPluginCatalog(Assembly assembly, Predicate<Type> filter = null, AssemblyPluginCatalogOptions options = null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
            _options = options ?? new AssemblyPluginCatalogOptions();

            if (filter != null)
            {
                var criteria = new TypeFinderCriteria();

                criteria.Query = (context, type) => filter(type);

                _options.TypeFinderCriterias.Add(string.Empty, criteria);
            }
        }

        public AssemblyPluginCatalog(string assemblyPath, Dictionary<string, Predicate<Type>> taggedFilters, AssemblyPluginCatalogOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath));
            }

            _assemblyPath = assemblyPath;
            _options = options ?? new AssemblyPluginCatalogOptions();

            foreach (var taggedFilter in taggedFilters)
            {
                var criteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };

                _options.TypeFinderCriterias.Add(taggedFilter.Key, criteria);
            }
        }

        public AssemblyPluginCatalog(Assembly assembly, Dictionary<string, Predicate<Type>> taggedFilters, AssemblyPluginCatalogOptions options = null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
            _options = options ?? new AssemblyPluginCatalogOptions();

            foreach (var taggedFilter in taggedFilters)
            {
                var criteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };

                _options.TypeFinderCriterias.Add(taggedFilter.Key, criteria);
            }
        }

        public Task Initialize()
        {
            if (!string.IsNullOrWhiteSpace(_assemblyPath) && _assembly == null)
            {
                if (!File.Exists(_assemblyPath))
                {
                    throw new ArgumentException($"Assembly in path {_assemblyPath} does not exist.");
                }

                _pluginAssemblyLoadContext = new PluginAssemblyLoadContext(_assemblyPath, _options.PluginLoadContextOptions);
                _assembly = _pluginAssemblyLoadContext.Load();
            }

            _pluginOld = AssemblyToPluginDefinitionConverter.Convert(_assembly, this);

            IsInitialized = true;

            return Task.CompletedTask;
        }

        public bool IsInitialized { get; private set; }

        public Task<List<PluginOld>> GetPluginsOld()
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            var result = new List<PluginOld>() { _pluginOld };

            return Task.FromResult(result);
        }

        public Task<PluginOld> GetPlugin()
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            return Task.FromResult(_pluginOld);
        }

        public Task<PluginOld> GetPlugin(string name, Version version)
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            if (!string.Equals(name, _pluginOld.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _pluginOld.Version)
            {
                return Task.FromResult<PluginOld>(null);
            }

            return Task.FromResult(_pluginOld);
        }

        public Task<Assembly> GetAssembly(PluginOld definition)
        {
            return Task.FromResult(_assembly);
        }

        public bool SupportsUnload { get; } = true;

        public Task Unload()
        {
            if (Unloaded)
            {
                return Task.CompletedTask;
            }

            _pluginAssemblyLoadContext.Unload();

            Unloaded = true;

            return Task.CompletedTask;
        }

        public bool Unloaded { get; private set; }

        public List<Plugin> GetPlugins()
        {
            var finder = new TypeFinder();

            var result = new List<Plugin>();
            
            foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
            {
                var pluginTypes = finder.Find(typeFinderCriteria.Value, _assembly, _pluginAssemblyLoadContext);

                foreach (var type in pluginTypes)
                {
                    var opt = new TypePluginCatalogOptions();
                    var version = opt.PluginVersionGenerator(opt, type);
                    var pluginName = opt.PluginNameGenerator(opt, type);
                    var description = opt.PluginDescriptionGenerator(opt, type);
                    var productVersion = opt.PluginProductVersionGenerator(opt, type);

                    var plugin = new Plugin(type.Assembly, type, pluginName, version, this, description, productVersion);
                    result.Add(plugin);
                }
            }

            return result;

            //
            // var allTypes = assembly.GetExportedTypes();
            // var taggedTypes = new List<(string Tag, Type Type)>();
            //
            // if (_filters?.Any() == true)
            // {
            //     foreach (var taggedFilter in _filters)
            //     {
            //         taggedTypes.AddRange(allTypes.Where(x => taggedFilter.Value(x)).Select(x => (taggedFilter.Key, x)));
            //     }
            // }
            // else
            // {
            //     taggedTypes.AddRange(allTypes.Select(x => (string.Empty, x)));
            // }
            //
            // var result = new List<Plugin>();
            //
            // foreach (var taggedType in taggedTypes)
            // {
            //     var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            //     var pluginName = taggedType.Type.FullName;
            //     var fileVersion = Version.Parse(versionInfo.FileVersion);
            //     var description = versionInfo.Comments;
            //     var productVersion = versionInfo.ProductVersion;
            //
            //     var p = new Plugin(_assembly, taggedType.Type, pluginName, fileVersion, this, description, productVersion, taggedType.Tag);
            //
            //     result.Add(p);
            // }
            //
            // return result;
        }

        public Plugin Get(string name, Version version)
        {
            throw new NotImplementedException();
        }
    }
}
