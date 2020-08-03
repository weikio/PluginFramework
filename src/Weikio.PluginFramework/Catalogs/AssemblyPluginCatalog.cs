using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalog : IPluginCatalog
    {
        private readonly string _assemblyPath;
        private Assembly _assembly;
        private readonly AssemblyPluginCatalogOptions _options;
        private PluginAssemblyLoadContext _pluginAssemblyLoadContext;
        private List<TypePluginCatalog> _plugins = null;

        public AssemblyPluginCatalog(string assemblyPath) : this(assemblyPath, null, null, null)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly) : this(null, assembly)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, AssemblyPluginCatalogOptions options = null) : this(assemblyPath, null, null, null, null, null, options)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly, AssemblyPluginCatalogOptions options = null) : this(null, assembly, null, null, null, null, options)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, TypeFinderCriteria criteria = null, AssemblyPluginCatalogOptions options = null) : this(assemblyPath,
            null, null, null,
            null, criteria, options)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, Action<TypeFinderCriteriaBuilder> configureFinder = null,
            AssemblyPluginCatalogOptions options = null) : this(assemblyPath,
            null, null, null, configureFinder, null, options)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly, Action<TypeFinderCriteriaBuilder> configureFinder = null,
            AssemblyPluginCatalogOptions options = null) : this(null,
            assembly, null, null, configureFinder, null, options)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, Predicate<Type> filter = null, AssemblyPluginCatalogOptions options = null) : this(assemblyPath, null,
            filter, null, null, null, options)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly, Predicate<Type> filter = null, AssemblyPluginCatalogOptions options = null) : this(null, assembly,
            filter, null, null, null, options)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath, Dictionary<string, Predicate<Type>> taggedFilters,
            AssemblyPluginCatalogOptions options = null) : this(assemblyPath, null, null, taggedFilters, null, null, options)
        {
        }

        public AssemblyPluginCatalog(Assembly assembly, Dictionary<string, Predicate<Type>> taggedFilters,
            AssemblyPluginCatalogOptions options = null) : this(null, assembly, null, taggedFilters, null, null, options)
        {
        }

        public AssemblyPluginCatalog(string assemblyPath = null, Assembly assembly = null, Predicate<Type> filter = null,
            Dictionary<string, Predicate<Type>> taggedFilters = null, Action<TypeFinderCriteriaBuilder> configureFinder = null,
            TypeFinderCriteria criteria = null, AssemblyPluginCatalogOptions options = null)
        {
            if (assembly != null)
            {
                _assembly = assembly;
                _assemblyPath = _assembly.Location;
            }
            else if (!string.IsNullOrWhiteSpace(assemblyPath))
            {
                _assemblyPath = assemblyPath;
            }
            else
            {
                throw new ArgumentNullException($"{nameof(assembly)} or {nameof(assemblyPath)} must be set.");
            }
                
            _options = options ?? new AssemblyPluginCatalogOptions();

            SetFilters(filter, taggedFilters, criteria, configureFinder);
        }

        private void SetFilters(Predicate<Type> filter, Dictionary<string, Predicate<Type>> taggedFilters, TypeFinderCriteria criteria,
            Action<TypeFinderCriteriaBuilder> configureFinder)
        {
            if (_options.TypeFinderCriterias == null)
            {
                _options.TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
            }
            
            if (filter != null)
            {
                var filterCriteria = new TypeFinderCriteria { Query = (context, type) => filter(type) };

                _options.TypeFinderCriterias.Add(string.Empty, filterCriteria);
            }

            if (taggedFilters?.Any() == true)
            {
                foreach (var taggedFilter in taggedFilters)
                {
                    var taggedCriteria = new TypeFinderCriteria { Query = (context, type) => taggedFilter.Value(type) };

                    _options.TypeFinderCriterias.Add(taggedFilter.Key, taggedCriteria);
                }
            }

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var configuredCriteria = builder.Build();

                _options.TypeFinderCriterias.Add("", configuredCriteria);
            }

            if (criteria != null)
            {
                _options.TypeFinderCriterias.Add("", criteria);
            }
            
            if (_options.TypeFinderCriterias?.Any() != true)
            {
                var findAll = TypeFinderCriteriaBuilder
                    .Create()
                    .Build();

                _options.TypeFinderCriterias.Add(string.Empty, findAll);
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
