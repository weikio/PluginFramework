using System;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class DependencyResolutionTests
    {
        [Fact]
        public async Task PluginsCanUseReferencedDependencies()
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
        public async Task PluginsCanUseHoststDependencies()
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
        
        [Fact]
        public async Task AssemblyPluginsCanUseHoststDependencies()
        {
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);

            var options = new AssemblyPluginCatalogOptions();
            options.PluginLoadContextOptions = new PluginLoadContextOptions() { UseHostApplicationAssemblies = true };

            var folder1Catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0\JsonNetNew.dll", options);
            await folder1Catalog.Initialize();

            var folder2Catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0\JsonNetOld.dll", options);
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
