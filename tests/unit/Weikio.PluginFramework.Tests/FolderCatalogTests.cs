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
        private static readonly string _pluginFolder = @"..\..\..\..\..\Assemblies\bin\net7.0".Replace(@"\",Path.DirectorySeparatorChar.ToString());

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
        public async Task CanUseFolderOptions()
        {
            var options = new FolderPluginCatalogOptions
            {
                TypeFinderOptions = new TypeFinderOptions()
                {
                    TypeFinderCriterias = new List<TypeFinderCriteria>()
                    {
                        TypeFinderCriteriaBuilder
                            .Create()
                            .HasName("SecondPlugin")
                            .Tag("MyPlugin"),
                    }
                }
            };

            var catalog = new FolderPluginCatalog(_pluginFolder, options);

            await catalog.Initialize();

            var pluginCount = catalog.GetPlugins().Count;

            Assert.Equal(1, pluginCount);
            Assert.Equal("SecondPlugin", catalog.Single().Type.Name);
        }

        [Fact]
        public async Task FolderOptionsAreUsedToLimitLoadedAssemblies()
        {
            var options = new FolderPluginCatalogOptions
            {
                TypeFinderOptions = new TypeFinderOptions()
                {
                    TypeFinderCriterias = new List<TypeFinderCriteria>()
                    {
                        TypeFinderCriteriaBuilder
                            .Create()
                            .HasName("SecondPlugin")
                            .Tag("MyPlugin"),
                    }
                }
            };

            var catalog = new FolderPluginCatalog(_pluginFolder, options);

            await catalog.Initialize();

            var field = catalog.GetType().GetField("_catalogs", BindingFlags.Instance | BindingFlags.NonPublic);
            // ReSharper disable once PossibleNullReferenceException
            var loadedAssemblies = (List<AssemblyPluginCatalog>) field.GetValue(catalog);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Single(loadedAssemblies);
        }

        [Fact]
        public async Task CanConfigureNamingOptions()
        {
            var options = new FolderPluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified" }
            };

            var catalog = new FolderPluginCatalog(_pluginFolder, options);
            await catalog.Initialize();

            var plugins = catalog.GetPlugins();

            foreach (var plugin in plugins)
            {
                Assert.EndsWith("Modified", plugin.Name);
            }
        }

        [Fact]
        public async Task CanUseReferencedDependencies()
        {
            PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Never;

            Action<TypeFinderCriteriaBuilder> configureFinder = configure =>
            {
                configure.HasName("*JsonResolver");
            };

            var folder1Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\net7.0".Replace(@"\",Path.DirectorySeparatorChar.ToString()), configureFinder);
            var folder2Catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\net7.0".Replace(@"\",Path.DirectorySeparatorChar.ToString()), configureFinder);

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
                    HostApplicationAssemblies = new List<AssemblyName>()
                    {
                        typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly.GetName(),
                        typeof(Newtonsoft.Json.JsonConvert).Assembly.GetName()
                    }
                }
            };
            var hostLogAssemblyVersion = typeof(Microsoft.Extensions.Logging.LoggerFactory)
                .Assembly.GetName().Version?.ToString();
            var hostJsonAssemblyVersion = typeof(Newtonsoft.Json.JsonConvert)
                .Assembly.GetName().Version?.ToString();
            var catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\net7.0".Replace(@"\",Path.DirectorySeparatorChar.ToString()), options);
            await catalog.Initialize();

            var oldPlugin = catalog.Single();

            dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin);
            var oldPluginVersion = oldPluginJsonResolver.GetVersion();
            var loggerVersion = oldPluginJsonResolver.GetLoggingVersion();

            Assert.Equal(hostLogAssemblyVersion, loggerVersion);
            Assert.Equal(hostJsonAssemblyVersion, oldPluginVersion);
        }

        [Collection(nameof(NotThreadSafeResourceCollection))]
        public class DefaultOptions : IDisposable
        {
            public DefaultOptions()
            {
                FolderPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
                {
                    PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified"
                };
            }

            [Fact]
            public async Task CanConfigureDefaultNamingOptions()
            {
                var catalog = new FolderPluginCatalog(_pluginFolder);
                await catalog.Initialize();

                var plugins = catalog.GetPlugins();

                foreach (var plugin in plugins)
                {
                    Assert.EndsWith("Modified", plugin.Name);
                }
            }

            [Fact]
            public async Task DefaultAssemblyNamingOptionsDoesntAffectFolderCatalogs()
            {
                AssemblyPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
                {
                    PluginNameGenerator = (nameOptions, type) => type.FullName + "ModifiedAssembly"
                };

                var catalog = new FolderPluginCatalog(_pluginFolder);
                await catalog.Initialize();

                var plugins = catalog.GetPlugins();

                foreach (var plugin in plugins)
                {
                    Assert.False(plugin.Name.EndsWith("ModifiedAssembly"));
                }
            }

            public void Dispose()
            {
                AssemblyPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions();
                FolderPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions();
            }
        }
    }
}
