using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class FolderCatalogTests
    {
        private const string _pluginFolder = @"..\..\..\..\..\Assemblies\bin\netstandard2.0";

        [Fact]
        public async Task CanInitialize()
        {
            var catalog = new FolderPluginCatalog(_pluginFolder);
            await catalog.Initialize();
            
            var plugins = catalog.GetPlugins();
            
            Assert.NotEmpty(plugins);
        }

        [Fact]
        public async Task CanInitializeWithCriteria()
        {
            var catalog = new FolderPluginCatalog(_pluginFolder, configure =>
            {
                configure.HasName("*Plugin");
            });
            
            await catalog.Initialize();

            var pluginCount = catalog.GetPlugins().Count;
            
            Assert.Equal(2, pluginCount);
        }

        [Fact]
        public async Task CanUseReferencedDependencies()
        {
            PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Never;
            
            Action<TypeFinderCriteriaBuilder> configureFinder = configure =>
            {
                configure.HasName("*JsonResolver");
            };
            
            var folder1Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0", configureFinder);
            var folder2Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0", configureFinder);
            
            await folder1Catalog.Initialize();
            await folder2Catalog.Initialize();

            var newPlugin = folder1Catalog.Single();
            var oldPlugin = folder2Catalog.Single();
            
            dynamic newPluginJsonResolver = Activator.CreateInstance(newPlugin);
            var newPluginVersion = newPluginJsonResolver.GetVersion();
            
            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin);
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();
            
            Assert.Equal("10.0.0.0", newPluginVersion);
            Assert.Equal("9.0.0.0", oldPluginVersion);
        }

        [Fact]
        public async Task CanUseSelectedHoststDependencies()
        {
            // Make sure that the referenced version of JSON.NET is loaded into memory
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);
            // Make sure that the referenced version of Microsoft.Extensions.Logging is loaded into memory
            var logging = new Microsoft.Extensions.Logging.LoggerFactory();

            var options = new FolderPluginCatalogOptions
            {
                TypeFinderCriteria = new TypeFinderCriteria() { Name = "*JsonResolver" },
                PluginLoadContextOptions = new PluginLoadContextOptions()
                {
                    UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Selected,
                    HostApplicationAssemblies = new List<AssemblyName>() { typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly.GetName() }
                }
            };

            var catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0", options);
            await catalog.Initialize();

            var oldPlugin = catalog.Single();
            
            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin);
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();
            var loggerVersion = oldPluginJsonResolver.GetLoggingVersion();
            
            Assert.Equal("3.1.2.0", loggerVersion);
            Assert.Equal("9.0.0.0", oldPluginVersion);
        }
    }
}
