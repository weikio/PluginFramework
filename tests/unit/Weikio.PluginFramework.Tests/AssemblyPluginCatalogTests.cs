using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
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
        public async Task CanUnload()
        {
            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\TestAssembly1.dll");
            await catalog.Initialize();

            var pluginDefinition = (await catalog.GetAll()).Single();

            var pluginExporter = new PluginExporter();
            var plugin = await pluginExporter.Get(pluginDefinition);

            dynamic jsonResolver = Activator.CreateInstance(plugin.Types.First());
            jsonResolver.RunMe();

            await catalog.Unload();

            await Assert.ThrowsAsync<CatalogUnloadedException>(async () => await pluginExporter.Get(pluginDefinition));
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
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);
            
            var assemblyCatalog1 = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0\JsonNetNew.dll");
            await assemblyCatalog1.Initialize();

            var assemblyCatalog2 = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0\JsonNetOld.dll");
            await assemblyCatalog2.Initialize();

            var newPluginDefinition = (await assemblyCatalog1.GetAll()).Single();
            var oldPluginDefinition = (await assemblyCatalog2.GetAll()).Single();

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
        public async Task CanUseHostsDependencies()
        {
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);

            var options = new AssemblyPluginCatalogOptions();
            options.PluginLoadContextOptions = new PluginLoadContextOptions() { UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always };

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
        
        [Fact]
        public async Task CanUseSelectedHoststDependencies()
        {
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);
            // Make sure that the referenced version of Microsoft.Extensions.Logging is loaded into memory
            var logging = new Microsoft.Extensions.Logging.LoggerFactory();

            var options = new AssemblyPluginCatalogOptions();;
            options.PluginLoadContextOptions = new PluginLoadContextOptions()
            {
                UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Selected,
                HostApplicationAssemblies = new List<AssemblyName>()
                {
                    typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly.GetName()
                }
            };

            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0\JsonNetOld.dll", options);
            await catalog.Initialize();

            var oldPluginDefinition = (await catalog.GetAll()).Single();

            var pluginExporter = new PluginExporter();
            var oldPlugin = await pluginExporter.Get(oldPluginDefinition);

            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin.Types.First());
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();
            var loggerVersion = oldPluginJsonResolver.GetLoggingVersion();
            
            Assert.Equal("3.1.2.0", loggerVersion);
            Assert.Equal("9.0.0.0", oldPluginVersion);
        }
    }
}
