using System;
using System.Collections.Generic;
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
        private readonly AssemblyPluginCatalogOptions _options;
        private PluginAssemblyLoadContext _pluginAssemblyLoadContext;
        private List<TypePluginCatalog> _plugins = null;

        // TODO: Remove the duplicate code from constructors
        public AssemblyPluginCatalog(string assemblyPath) : this(assemblyPath, options: null)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly) : this(assembly, options: null)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, AssemblyPluginCatalogOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath));
            }

            _assemblyPath = assemblyPath;
            _options = options ?? new AssemblyPluginCatalogOptions();
        }

        public AssemblyPluginCatalog(Assembly assembly, AssemblyPluginCatalogOptions options = null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
            _options = options ?? new AssemblyPluginCatalogOptions();
        }

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
        
        public AssemblyPluginCatalog(string assemblyPath, Action<TypeFinderCriteriaBuilder> configureFinder = null, AssemblyPluginCatalogOptions options = null)
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                throw new ArgumentNullException(nameof(assemblyPath));
            }

            _assemblyPath = assemblyPath;
            _options = options ?? new AssemblyPluginCatalogOptions();

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var criteria = builder.Build();

                _options.TypeFinderCriterias.Add("", criteria);
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
                var criteria = new TypeFinderCriteria { Query = (context, type) => filter(type) };

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
                var criteria = new TypeFinderCriteria { Query = (context, type) => filter(type) };

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

            SetFilters(taggedFilters);
        }

        public AssemblyPluginCatalog(Assembly assembly, Dictionary<string, Predicate<Type>> taggedFilters, AssemblyPluginCatalogOptions options = null)
        {
            _assembly = assembly;
            _assemblyPath = _assembly.Location;
            _options = options ?? new AssemblyPluginCatalogOptions();

            SetFilters(taggedFilters);
        }

        private void SetFilters(Dictionary<string, Predicate<Type>> taggedFilters)
        {
            foreach (var taggedFilter in taggedFilters)
            {
                var criteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };

                _options.TypeFinderCriterias.Add(taggedFilter.Key, criteria);
            }
        }

        public async Task Initialize()
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

            _plugins = new List<TypePluginCatalog>();

            var finder = new TypeFinder();

            if (_options.TypeFinderCriterias?.Any() != true)
            {
                var findAll = new TypeFinderCriteria()
                {
                    Query = (context, type) => true
                };

                if (_options.TypeFinderCriterias == null)
                {
                    _options.TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
                }
                
                _options.TypeFinderCriterias.Add(string.Empty, findAll);
            }

            foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
            {
                var pluginTypes = finder.Find(typeFinderCriteria.Value, _assembly, _pluginAssemblyLoadContext);

                foreach (var type in pluginTypes)
                {
                    var typePluginCatalog = new TypePluginCatalog(type, new TypePluginCatalogOptions() { PluginNameOptions = _options.PluginNameOptions });
                    await typePluginCatalog.Initialize();

                    _plugins.Add(typePluginCatalog);
                }
            }

            IsInitialized = true;
        }

        public bool IsInitialized { get; private set; }

        public List<Plugin> GetPlugins()
        {
            return _plugins.SelectMany(x => x.GetPlugins()).ToList();
        }

        public Plugin Get(string name, Version version)
        {
            foreach (var pluginCatalog in _plugins)
            {
                var foundPlugin = pluginCatalog.Get(name, version);

                if (foundPlugin == null)
                {
                    continue;
                }

                return foundPlugin;
            }

            return null;
        }
    }
}
