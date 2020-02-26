using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
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
        
        [Fact]
        public async Task CanUseReferencedDependencies()
        {
            var options = new FolderPluginCatalogOptions();

            options.AssemblyPluginResolvers.Add(assembly =>
            {
                var result = assembly.ExportedTypes.Any(x => x.Name.EndsWith("JsonResolver"));

                return result;
            });

            var folder1Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0", options);
            await folder1Catalog.Initialize();

            var folder2Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0", options);
            await folder2Catalog.Initialize();

            var newPluginDefinition = (await folder1Catalog.GetAll()).Single();
            var oldPluginDefinition = (await folder2Catalog.GetAll()).Single();

            var pluginExporter = new PluginExporter();
            var newPlugin = await pluginExporter.Get(newPluginDefinition);
            var oldPlugin = await pluginExporter.Get(oldPluginDefinition);

            dynamic newPluginJsonResolver = Activator.CreateInstance(newPlugin.Types.First());
            var newPluginVersion = newPluginJsonResolver.GetVersion();

            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin.Types.First());
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();

            Assert.Equal("12.0.0.0", newPluginVersion);
            Assert.Equal("9.0.0.0", oldPluginVersion);
        }

        [Fact]
        public async Task CanUseHoststDependencies()
        {
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);

            var options = new FolderPluginCatalogOptions();

            options.AssemblyPluginResolvers.Add(assembly =>
            {
                var result = assembly.ExportedTypes.Any(x => x.Name.EndsWith("JsonResolver"));

                return result;
            });

            options.PluginLoadContextOptions = new PluginLoadContextOptions() { UseHostApplicationAssemblies = true };

            var folder1Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0", options);
            await folder1Catalog.Initialize();

            var folder2Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0", options);
            await folder2Catalog.Initialize();

            var newPluginDefinition = (await folder1Catalog.GetAll()).Single();
            var oldPluginDefinition = (await folder2Catalog.GetAll()).Single();

            var pluginExporter = new PluginExporter();
            var newPlugin = await pluginExporter.Get(newPluginDefinition);
            var oldPlugin = await pluginExporter.Get(oldPluginDefinition);

            dynamic newPluginJsonResolver = Activator.CreateInstance(newPlugin.Types.First());
            var newPluginVersion = newPluginJsonResolver.GetVersion();

            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin.Types.First());
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();

            Assert.Equal("10.0.0.0", newPluginVersion);
            Assert.Equal("10.0.0.0", oldPluginVersion);
        }        
    }
}
