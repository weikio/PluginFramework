using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalog : IPluginCatalog
    {
        private readonly string _folderPath;
        private readonly List<(PluginDefinition PluginDefinition, Assembly Assembly)> _plugins = new List<(PluginDefinition, Assembly)>();
        private readonly FolderPluginCatalogOptions _options;

        public bool IsInitialized { get; private set; }

        public Task<List<PluginDefinition>> GetAll()
        {
            return Task.FromResult(_plugins.Select(x => x.PluginDefinition).ToList());
        }

        public FolderPluginCatalog(string folderPath, FolderPluginCatalogOptions options = null)
        {
            _folderPath = folderPath;
            _options = options ?? new FolderPluginCatalogOptions();
        }

        public Task<PluginDefinition> Get(string name, Version version)
        {
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

                var loadContext = new PluginLoadContext(assemblyPath);
                var assembly = loadContext.Load();

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
            
                    var referencePaths = new[]
                    {
                        coreLocation,
                        assemblyPath
                    };
            
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
                }

                if (_options.PluginResolvers?.Any() != true)
                {
                    return false;
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
