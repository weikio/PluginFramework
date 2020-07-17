using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalog : IPluginCatalog
    {
        private readonly string _folderPath;
        private readonly List<(PluginDefinition PluginDefinition, Assembly Assembly)> _plugins = new List<(PluginDefinition, Assembly)>();
        private readonly FolderPluginCatalogOptions _options;
        private readonly List<PluginLoadContext> _contexts = new List<PluginLoadContext>();

        public bool IsInitialized { get; private set; }

        public FolderPluginCatalog(string folderPath, FolderPluginCatalogOptions options = null)
        {
            _folderPath = folderPath;
            _options = options ?? new FolderPluginCatalogOptions();
        }

        public Task<List<PluginDefinition>> GetAll()
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }
            
            return Task.FromResult(_plugins.Select(x => x.PluginDefinition).ToList());
        }
        
        public Task<PluginDefinition> Get(string name, Version version)
        {
            if (Unloaded)
            {
                throw new CatalogUnloadedException();
            }

            foreach (var plugin in _plugins)
            {
                if (string.Equals(name, plugin.PluginDefinition.Name, StringComparison.InvariantCultureIgnoreCase) &&
                    version == plugin.PluginDefinition.Version)
                {
                    return Task.FromResult(plugin.PluginDefinition);
                }
            }

            return null;
        }

        public Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            foreach (var plugin in _plugins)
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
                if (!IsPlugin(assemblyPath))
                {
                    continue;
                }

                var loadContext = new PluginLoadContext(assemblyPath, _options.PluginLoadContextOptions);
                var assembly = loadContext.Load();
                _contexts.Add(loadContext);

                var definition = AssemblyToPluginDefinitionConverter.Convert(assembly, this);

                _plugins.Add((definition, assembly));
            }

            IsInitialized = true;

            return Task.CompletedTask;
        }

        private bool IsPlugin(string assemblyPath)
        {
            using (Stream stream = File.OpenRead(assemblyPath))
            using (var reader = new PEReader(stream))
            {
                if (!reader.HasMetadata)
                {
                    return false;
                }

                // First try to resolve plugin assemblies using MetadataLoadContext
                if (_options.AssemblyPluginResolvers?.Any() == true)
                {
                    var coreAssemblyPath = typeof(int).Assembly.Location;
                    var corePath = Path.GetDirectoryName(coreAssemblyPath);

                    var coreLocation = Path.Combine(corePath, "mscorlib.dll");

                    if (!File.Exists(coreLocation))
                    {
                        throw new FileNotFoundException(coreLocation);
                    }

                    var referencePaths = new[] { coreLocation, assemblyPath };

                    var resolver = new PathAssemblyResolver(referencePaths);

                    using (var metadataContext = new MetadataLoadContext(resolver))
                    {
                        var readonlyAssembly = metadataContext.LoadFromAssemblyPath(assemblyPath);

                        foreach (var assemblyPluginResolver in _options.AssemblyPluginResolvers)
                        {
                            if (assemblyPluginResolver(readonlyAssembly))
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
                    // If there are not resolvers, assume that each DLL is a plugin
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
}
