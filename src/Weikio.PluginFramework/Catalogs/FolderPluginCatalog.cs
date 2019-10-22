using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalog : IPluginCatalog
    {
        private readonly string _folderPath;
        private List<(PluginDefinition PluginDefinition, Assembly Assembly)> _plugins = new List<(PluginDefinition, Assembly)>();

        private readonly Func<MetadataReader, TypeDefinition, bool> _pluginResolver;

        public bool IsInitialized { get; private set; }

        public Task<List<PluginDefinition>> GetAll()
        {
            return Task.FromResult(_plugins.Select(x => x.PluginDefinition).ToList());
        }

        public FolderPluginCatalog(string folderPath, Func<MetadataReader, TypeDefinition, bool> pluginResolver = null)
        {
            _folderPath = folderPath;
            _pluginResolver = pluginResolver;
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
            var dllFiles = Directory.GetFiles(_folderPath, "*.dll");

            foreach (var assemblyPath in dllFiles)
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

                if (_pluginResolver == null)
                {
                    return true;
                }

                var metadata = reader.GetMetadataReader();

                var publicTypes = metadata.TypeDefinitions
                    .Select(metadata.GetTypeDefinition)
                    .Where(t => t.Attributes.HasFlag(TypeAttributes.Public))
                    .ToArray();

                foreach (var type in publicTypes)
                {
                    if (!_pluginResolver(metadata, type))
                    {
                        continue;
                    }

                    return true;
                }
            }

            return false;
        }
    }
}
