using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using AssemblyExtensions = System.Reflection.AssemblyExtensions;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalog : IPluginCatalog
    {
        private readonly string _folderPath;
        private readonly List<(PluginOld PluginDefinition, Assembly Assembly)> _pluginsOld = new List<(PluginOld, Assembly)>();
        private readonly List<Plugin> _plugins = new List<Plugin>();
        private readonly FolderPluginCatalogOptions _options;
        private readonly List<PluginAssemblyLoadContext> _contexts = new List<PluginAssemblyLoadContext>();

        public bool IsInitialized { get; private set; }

        public FolderPluginCatalog(string folderPath, TypeFinderCriteria finderCriteria = null, FolderPluginCatalogOptions options = null)
        {
            _folderPath = folderPath;
            _options = options ?? new FolderPluginCatalogOptions();

            if (finderCriteria != null)
            {
                _options.TypeFinderCriterias.Add("", finderCriteria);
            }
        }

        public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder = null, FolderPluginCatalogOptions options = null)
        {
            _folderPath = folderPath;
            _options = options ?? new FolderPluginCatalogOptions();

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var criteria = builder.Build();

                _options.TypeFinderCriterias.Add("", criteria);
            }
        }

        public Task<List<PluginOld>> GetPluginsOld()
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            return Task.FromResult(_pluginsOld.Select(x => x.PluginDefinition).ToList());
        }

        public Task<PluginOld> GetPlugin(string name, Version version)
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            foreach (var plugin in _pluginsOld)
            {
                if (string.Equals(name, plugin.PluginDefinition.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    version == plugin.PluginDefinition.Version)
                {
                    return Task.FromResult(plugin.PluginDefinition);
                }
            }

            return null;
        }

        public Task<Assembly> GetAssembly(PluginOld definition)
        {
            foreach (var plugin in _pluginsOld)
            {
                if (string.Equals(definition.Name, plugin.PluginDefinition.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    definition.Version == plugin.PluginDefinition.Version)
                {
                    return Task.FromResult(plugin.Assembly);
                }
            }

            return null;
        }

        public bool SupportsUnload { get; } = true;

        public Task Unload()
        {
            foreach (var pluginLoadContext in _contexts)
            {
                pluginLoadContext.Unload();
            }

            _contexts.Clear();

            Unloaded = true;

            return Task.CompletedTask;
        }

        public bool Unloaded { get; private set; }

        public List<Plugin> GetPlugins()
        {
            return _plugins;
        }

        public Plugin Get(string name, Version version)
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            foreach (var plugin in _plugins)
            {
                if (string.Equals(name, plugin.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    version == plugin.Version)
                {
                    return plugin;
                }
            }

            return null;
        }

        public Task Initialize()
        {
            var foundFiles = new List<string>();

            foreach (var searchPattern in _options.SearchPatterns)
            {
                var dllFiles = Directory.GetFiles(_folderPath, searchPattern,
                    _options.IncludSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foundFiles.AddRange(dllFiles);
            }

            foundFiles = foundFiles.Distinct().ToList();

            foreach (var assemblyPath in foundFiles)
            {
                var isPluginAssembly = IsPluginAssembly(assemblyPath);

                if (isPluginAssembly == false)
                {
                    continue;
                }

                var loadContext = new PluginAssemblyLoadContext(assemblyPath, _options.PluginLoadContextOptions);
                var assembly = loadContext.Load();
                _contexts.Add(loadContext);

                var finder = new TypeFinder();

                foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
                {

                    var pluginTypes = finder.Find(typeFinderCriteria.Value, assembly, loadContext);

                    foreach (var type in pluginTypes)
                    {
                        var opt = new TypePluginCatalogOptions();
                        var version = opt.PluginVersionGenerator(opt, type);
                        var pluginName = opt.PluginNameGenerator(opt, type);
                        var description = opt.PluginDescriptionGenerator(opt, type);
                        var productVersion = opt.PluginProductVersionGenerator(opt, type);

                        var plugin = new Plugin(type.Assembly, type, pluginName, version, this, description, productVersion);
                        _plugins.Add(plugin);
                    }
                }
            }

            IsInitialized = true;

            return Task.CompletedTask;
        }

        private bool IsPluginAssembly(string assemblyPath)
        {
            using (Stream stream = File.OpenRead(assemblyPath))
            using (var reader = new PEReader(stream))
            {
                if (!reader.HasMetadata)
                {
                    return false;
                }

                // First try to resolve plugin assemblies using MetadataLoadContext
                if (_options.TypeFinderCriterias?.Any() == true)
                {
                    var coreAssemblyPath = typeof(int).Assembly.Location;
                    var corePath = Path.GetDirectoryName(coreAssemblyPath);

                    var coreLocation = Path.Combine(corePath, "mscorlib.dll");

                    if (!File.Exists(coreLocation))
                    {
                        throw new FileNotFoundException(coreLocation);
                    }

                    var runtimeAssemblies = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
                    var paths = new List<string>(runtimeAssemblies);
                    paths.Add(assemblyPath);

                    if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
                    {
                        var hostApplicationPath = Environment.CurrentDirectory;
                        var hostDlls = Directory.GetFiles(hostApplicationPath, "*.dll", SearchOption.AllDirectories);

                        paths.AddRange(hostDlls);
                    }
                    else if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Never)
                    {
                        var pluginPath = Path.GetDirectoryName(assemblyPath);
                        var dllsInPluginPath = Directory.GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories);

                        paths.AddRange(dllsInPluginPath);
                    }
                    else if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Selected)
                    {
                        foreach (var hostApplicationAssembly in _options.PluginLoadContextOptions.HostApplicationAssemblies)
                        {
                            var assembly = Assembly.Load(hostApplicationAssembly);
                            paths.Add(assembly.Location);
                        }
                    }

                    var resolver = new PathAssemblyResolver(paths);

                    using (var metadataContext = new MetadataLoadContext(resolver))
                    {
                        var metadataPluginLoadContext = new MetadataTypeFindingContext(metadataContext);
                        var readonlyAssembly = metadataContext.LoadFromAssemblyPath(assemblyPath);

                        var typeFinder = new TypeFinder();

                        foreach (var finderCriteria in _options.TypeFinderCriterias)
                        {
                            var typesFound = typeFinder.Find(finderCriteria.Value, readonlyAssembly, metadataPluginLoadContext);

                            if (typesFound?.Any() == true)
                            {
                                return true;
                            }
                        }
                    }

                    if (_options.PluginResolvers.Any() != true)
                    {
                        // If there is assembly resolvers but no plugin resolvers, return false by default if not assembly resolver did not match.
                        return false;
                    }
                }

                if (_options.PluginResolvers?.Any() != true)
                {
                    // If there are no resolvers, assume that each DLL is a plugin
                    return true;
                }

                // Then using the PEReader
                var metadata = reader.GetMetadataReader();

                var publicTypes = metadata.TypeDefinitions
                    .Select(metadata.GetTypeDefinition)
                    .Where(t => t.Attributes.HasFlag(TypeAttributes.Public))
                    .ToArray();

                foreach (var type in publicTypes)
                {
                    foreach (var pluginResolver in _options.PluginResolvers)
                    {
                        if (pluginResolver(assemblyPath, metadata, type))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    public class TypeFinder
    {
        public List<Type> Find(TypeFinderCriteria criteria, Assembly assembly, ITypeFindingContext typeFindingContext)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            var result = new List<Type>();
            var types = assembly.GetExportedTypes();

            foreach (var type in types)
            {
                if (criteria.Query != null)
                {
                    var isMatch = criteria.Query(typeFindingContext, type);

                    if (isMatch == false)
                    {
                        continue;
                    }

                    result.Add(type);

                    continue;
                }

                if (criteria.IsAbstract != null)
                {
                    if (type.IsAbstract != criteria.IsAbstract.GetValueOrDefault())
                    {
                        continue;
                    }
                }

                if (criteria.IsInterface != null)
                {
                    if (type.IsInterface != criteria.IsInterface.GetValueOrDefault())
                    {
                        continue;
                    }
                }

                if (string.IsNullOrWhiteSpace(criteria.Name) == false)
                {
                    var regEx = new Regex(criteria.Name, RegexOptions.Compiled);

                    if (regEx.IsMatch(type.FullName) == false)
                    {
                        continue;
                    }
                }

                if (criteria.Inherits != null)
                {
                    var inheritedType = typeFindingContext.FindType(criteria.Inherits);

                    if (inheritedType.IsAssignableFrom(type) == false)
                    {
                        continue;
                    }
                }

                if (criteria.Implements != null)
                {
                    var interfaceType = typeFindingContext.FindType(criteria.Implements);

                    if (interfaceType.IsAssignableFrom(type) == false)
                    {
                        continue;
                    }
                }

                result.Add(type);
            }

            return result;
        }
    }

    public class TypeFinderCriteria
    {
        public Type Inherits { get; set; }
        public Type Implements { get; set; }
        public bool? IsAbstract { get; set; }
        public bool? IsInterface { get; set; }
        public string Name { get; set; }
        public Func<ITypeFindingContext, Type, bool> Query { get; set; }
    }

    public class TypeFinderCriteriaBuilder
    {
        private Type _inherits;
        private Type _implements;
        private bool _isAbstract;
        private bool _isInterface;
        private string _name;

        public TypeFinderCriteria Build()
        {
            var res = new TypeFinderCriteria
            {
                IsInterface = _isInterface,
                Implements = _implements,
                Inherits = _inherits,
                Name = _name,
                IsAbstract = _isAbstract
            };

            return res;
        }

        public static implicit operator TypeFinderCriteria(TypeFinderCriteriaBuilder criteriaBuilder)
        {
            return criteriaBuilder.Build();
        }

        public static TypeFinderCriteriaBuilder Create()
        {
            return new TypeFinderCriteriaBuilder();
        }

        public TypeFinderCriteriaBuilder HasName(string name)
        {
            _name = name;

            return this;
        }

        public TypeFinderCriteriaBuilder Implements<T>()
        {
            return Implements(typeof(T));
        }

        public TypeFinderCriteriaBuilder Implements(Type t)
        {
            _implements = t;

            return this;
        }

        public TypeFinderCriteriaBuilder Inherits<T>()
        {
            return Inherits(typeof(T));
        }

        public TypeFinderCriteriaBuilder Inherits(Type t)
        {
            _inherits = t;

            return this;
        }

        public TypeFinderCriteriaBuilder IsAbstract(bool isAbstract)
        {
            _isAbstract = isAbstract;

            return this;
        }

        public TypeFinderCriteriaBuilder IsInterface(bool isInterface)
        {
            _isInterface = isInterface;

            return this;
        }
    }

    public interface ITypeFindingContext
    {
        Assembly FindAssembly(string assemblyName);
        Type FindType(Type type);
    }

    public class MetadataTypeFindingContext : ITypeFindingContext
    {
        private readonly MetadataLoadContext _metadataLoadContext;

        public MetadataTypeFindingContext(MetadataLoadContext metadataLoadContext)
        {
            _metadataLoadContext = metadataLoadContext;
        }

        public Assembly FindAssembly(string assemblyName)
        {
            var result = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

            return result;
        }

        public Type FindType(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            var assembly = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

            var result = assembly.GetType(type.FullName);

            return result;
        }
    }
}
