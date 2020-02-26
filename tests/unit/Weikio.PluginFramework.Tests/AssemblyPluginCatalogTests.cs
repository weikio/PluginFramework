using System;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class AssemblyPluginCatalogTests
    {
        [Fact]
        public async Task CanInitialize()
        {
            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\TestAssembly1.dll");
            await catalog.Initialize();

            var allPlugins = await catalog.GetAll();

            Assert.Single(allPlugins);
        }
        
        [Fact]
        public async Task ThrowsIfAssemblyNotFound()
        {
            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\notexists.dll");

            await Assert.ThrowsAsync<ArgumentException>(async () => await catalog.Initialize());
        }
        
        [Fact]
        public void ThrowsIfAssemblyPathMissing()
        {
            Assert.Throws<ArgumentNullException>(() => new AssemblyPluginCatalog(""));

            string path = null;
            Assert.Throws<ArgumentNullException>(() => new AssemblyPluginCatalog(path));
        }
        
        [Fact]
        public async Task CanUseReferencedDependencies()
        {
            var folder1Catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0\JsonNetNew.dll");
            await folder1Catalog.Initialize();

            var folder2Catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0\JsonNetOld.dll");
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
