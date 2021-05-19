using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    /// <summary>
    /// Plugin folder for a single folder (including or excluding subfolders). Locates the plugins from the assemblies (by default this means dll-files).
    /// </summary>
    public class FolderPluginCatalog : IPluginCatalog
    {
        private readonly string _folderPath;
        private readonly FolderPluginCatalogOptions _options;
        private readonly List<AssemblyPluginCatalog> _catalogs = new List<AssemblyPluginCatalog>();

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        private List<Plugin> Plugins
        {
            get
            {
                return _catalogs.SelectMany(x => x.GetPlugins()).ToList();
            }
        }

        public FolderPluginCatalog(string folderPath) : this(folderPath, new FolderPluginCatalogOptions())
        {
        }

        public FolderPluginCatalog(string folderPath, FolderPluginCatalogOptions options) : this(folderPath, null, null, options)
        {
        }

        public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder) : this(folderPath, configureFinder, null, null)
        {
        }

        public FolderPluginCatalog(string folderPath, TypeFinderCriteria finderCriteria) : this(folderPath, finderCriteria, null)
        {
        }

        public FolderPluginCatalog(string folderPath, TypeFinderCriteria finderCriteria, FolderPluginCatalogOptions options) : this(folderPath, null,
            finderCriteria, options)
        {
        }

        public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder, FolderPluginCatalogOptions options) : this(folderPath,
            configureFinder, null, options)
        {
        }

        public FolderPluginCatalog(string folderPath, Action<TypeFinderCriteriaBuilder> configureFinder, TypeFinderCriteria finderCriteria,
            FolderPluginCatalogOptions options)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentNullException(nameof(folderPath));
            }

            _folderPath = folderPath;
            _options = options ?? new FolderPluginCatalogOptions();

            if (_options.TypeFinderOptions == null)
            {
                _options.TypeFinderOptions = new TypeFinderOptions();
            }

            if (_options.TypeFinderOptions.TypeFinderCriterias == null)
            {
                _options.TypeFinderOptions.TypeFinderCriterias = new List<TypeFinderCriteria>();
            }

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var criteria = builder.Build();

                _options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);
            }

            if (finderCriteria != null)
            {
                _options.TypeFinderOptions.TypeFinderCriterias.Add(finderCriteria);
            }

            if (_options.TypeFinderCriteria != null)
            {
                _options.TypeFinderOptions.TypeFinderCriterias.Add(_options.TypeFinderCriteria);
            }

            if (_options.TypeFinderCriterias?.Any() == true)
            {
                foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
                {
                    var crit = typeFinderCriteria.Value;
                    crit.Tags = new List<string>() { typeFinderCriteria.Key };

                    _options.TypeFinderOptions.TypeFinderCriterias.Add(crit);
                }
            }
        }

        /// <inheritdoc />
        public List<Plugin> GetPlugins()
        {
            return Plugins;
        }

        /// <inheritdoc />
        public Plugin Get(string name, Version version)
        {
            foreach (var assemblyPluginCatalog in _catalogs)
            {
                var plugin = assemblyPluginCatalog.Get(name, version);

                if (plugin == null)
                {
                    continue;
                }

                return plugin;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task Initialize()
        {
            var foundFiles = new List<string>();

            foreach (var searchPattern in _options.SearchPatterns)
            {
                var dllFiles = Directory.GetFiles(_folderPath, searchPattern,
                    _options.IncludeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                foundFiles.AddRange(dllFiles);
            }

            foundFiles = foundFiles.Distinct().ToList();

            foreach (var assemblyPath in foundFiles)
            {
                // Assemblies are treated as readonly as long as possible
                var isPluginAssembly = IsPluginAssembly(assemblyPath);

                if (isPluginAssembly == false)
                {
                    continue;
                }

                var assemblyCatalogOptions = new AssemblyPluginCatalogOptions
                {
                    PluginLoadContextOptions = _options.PluginLoadContextOptions,
                    TypeFinderOptions = _options.TypeFinderOptions,
                    PluginNameOptions = _options.PluginNameOptions
                };

                // We are actually just delegating the responsibility from FolderPluginCatalog to AssemblyPluginCatalog. 
                var assemblyCatalog = new AssemblyPluginCatalog(assemblyPath, assemblyCatalogOptions);
                await assemblyCatalog.Initialize();

                _catalogs.Add(assemblyCatalog);
            }

            IsInitialized = true;
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

                if (_options.TypeFinderOptions?.TypeFinderCriterias?.Any() != true)
                {
                    // If there are no type finders configured, assume that each DLL is a plugin
                    return true;
                }

                var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
                var runtimeAssemblies = Directory.GetFiles(runtimeDirectory, "*.dll");
                var paths = new List<string>(runtimeAssemblies) { assemblyPath };

                if (_options.PluginLoadContextOptions.AdditionalRuntimePaths?.Any() == true)
                {
                    foreach (var additionalRuntimePath in _options.PluginLoadContextOptions.AdditionalRuntimePaths)
                    {
                        var dlls = Directory.GetFiles(additionalRuntimePath, "*.dll");
                        paths.AddRange(dlls);
                    }
                }

                if (_options.PluginLoadContextOptions.UseHostApplicationAssemblies == UseHostApplicationAssembliesEnum.Always)
                {
                    var hostApplicationPath = Environment.CurrentDirectory;
                    var hostDlls = Directory.GetFiles(hostApplicationPath, "*.dll", SearchOption.AllDirectories);

                    paths.AddRange(hostDlls);

                    AddSharedFrameworkDlls(hostApplicationPath, runtimeDirectory, paths);
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

                paths = paths.Distinct().ToList();

                // Also make sure to include only one dll of each. If same dll is found from multiple locations, use the first found dll and remove the others.
                var duplicateDlls = paths.Select(x => new {FullPath = x, FileName = Path.GetFileName(x)}).GroupBy(x => x.FileName)
                    .Where(x => x.Count() > 1)
                    .ToList();

                var removed = new List<string>();

                foreach (var duplicateDll in duplicateDlls)
                {
                    foreach (var duplicateDllPath in duplicateDll.Skip(1))
                    {
                        removed.Add(duplicateDllPath.FullPath);
                    }
                }

                foreach (var re in removed)
                {
                    paths.Remove(re);
                }

                var resolver = new PathAssemblyResolver(paths);

                // We use the metadata (readonly) versions of the assemblies before loading them
                using (var metadataContext = new MetadataLoadContext(resolver))
                {
                    var metadataPluginLoadContext = new MetadataTypeFindingContext(metadataContext);
                    var readonlyAssembly = metadataContext.LoadFromAssemblyPath(assemblyPath);

                    var typeFinder = new TypeFinder();

                    foreach (var finderCriteria in _options.TypeFinderOptions.TypeFinderCriterias)
                    {
                        var typesFound = typeFinder.Find(finderCriteria, readonlyAssembly, metadataPluginLoadContext);

                        if (typesFound?.Any() == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void AddSharedFrameworkDlls(string hostApplicationPath, string runtimeDirectory, List<string> paths)
        {
            // Fixing #23. If the main application references a shared framework (for example WinForms), we want to add these dlls also
            var defaultAssemblies = AssemblyLoadContext.Default.Assemblies.ToList();

            var defaultAssemblyDirectories = defaultAssemblies.Where(x => x.IsDynamic == false).Where(x => string.IsNullOrWhiteSpace(x.Location) == false)
                .GroupBy(x => Path.GetDirectoryName(x.Location)).Select(x => x.Key).ToList();

            foreach (var assemblyDirectory in defaultAssemblyDirectories)
            {
                if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), hostApplicationPath.TrimEnd('\\').TrimEnd('/')))
                {
                    continue;
                }

                if (string.Equals(assemblyDirectory.TrimEnd('\\').TrimEnd('/'), runtimeDirectory.TrimEnd('\\').TrimEnd('/')))
                {
                    continue;
                }

                if (_options.PluginLoadContextOptions.AdditionalRuntimePaths == null)
                {
                    _options.PluginLoadContextOptions.AdditionalRuntimePaths = new List<string>();
                }

                if (_options.PluginLoadContextOptions.AdditionalRuntimePaths.Contains(assemblyDirectory) == false)
                {
                    _options.PluginLoadContextOptions.AdditionalRuntimePaths.Add(assemblyDirectory);
                }

                var dlls = Directory.GetFiles(assemblyDirectory, "*.dll");
                paths.AddRange(dlls);
            }
        }
    }
}
