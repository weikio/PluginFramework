using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Catalogs;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class FolderCatalogTests
    {
        private const string _pluginFolder = @"..\..\..\..\..\Assemblies\bin\netstandard2.0";

        [Fact]
        public async Task CanInitialize()
        {
            var plugins = new FolderPluginCatalog(_pluginFolder);
            await plugins.Initialize();
            
            var dllCount = Directory.GetFiles(_pluginFolder, "*.dll").Length;
            var pluginCount = (await plugins.GetAll()).Count;
            
            Assert.Equal(dllCount, pluginCount);
        }
        
        [Fact]
        public async Task CanInitializeWithPluginResolver()
        {
            var options = new FolderPluginCatalogOptions();

            options.PluginResolvers.Add((assembly, metadata, type) =>
            {
                var typeName = metadata.GetString(type.Name);

                if (typeName.EndsWith("Plugin"))
                {
                    return true;
                }

                return false;
            });

            var plugins = new FolderPluginCatalog(_pluginFolder, options);
            await plugins.Initialize();
            
            var pluginCount = (await plugins.GetAll()).Count;
            
            Assert.Equal(2, pluginCount);
        }
        
        [Fact]
        public async Task CanInitializeWithMultiplePluginResolver()
        {
            var options = new FolderPluginCatalogOptions();

            options.PluginResolvers.Add((assembly, metadata, type) =>
            {
                var typeName = metadata.GetString(type.Name);

                if (typeName.EndsWith("Plugin"))
                {
                    return true;
                }

                return false;
            });

            options.PluginResolvers.Add((assembly, metadata, type) => assembly.ToLowerInvariant().EndsWith("testassembly3.dll"));

            var plugins = new FolderPluginCatalog(_pluginFolder, options);
            await plugins.Initialize();
            
            var pluginCount = (await plugins.GetAll()).Count;
            
            Assert.Equal(3, pluginCount);
        }
        
        [Fact]
        public async Task CanInitializeWithAssemblyPluginResolver()
        {
            var options = new FolderPluginCatalogOptions();

            options.AssemblyPluginResolvers.Add(assembly =>
            {
                var types = assembly.GetExportedTypes();

                if (types.Any(x => x.Name.EndsWith("Plugin")))
                {
                    return true;
                }

                return false;
            });

            var plugins = new FolderPluginCatalog(_pluginFolder, options);
            await plugins.Initialize();
            
            var pluginCount = (await plugins.GetAll()).Count;
            
            Assert.Equal(2, pluginCount);
        }
    }
}
