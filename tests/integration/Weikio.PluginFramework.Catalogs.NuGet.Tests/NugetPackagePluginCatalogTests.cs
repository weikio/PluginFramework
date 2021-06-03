using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Weikio.NugetDownloader;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.TypeFinding;
using Xunit;

namespace PluginFramework.Catalogs.NuGet.Tests
{
    public class NugetPackagePluginCatalogTests
    {
        // this folder is defined in test project's bin so that NuGet implementation
        // will use the NuGet.Config defined in the test project root.
        private readonly string _packagesFolderInTestsBin;

        public NugetPackagePluginCatalogTests()
        {
            var executingAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _packagesFolderInTestsBin = Path.Combine(executingAssemblyDir, "TestPackages");
        }

        private void AssertAssemblyFrameWork(string targetFramework, Assembly assembly)
        {
            var assemblyFramework = assembly
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            Assert.Equal(targetFramework, assemblyFramework);
        }

        [Fact]
        public async Task InstallPackageWithoutDepencencies()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", configureFinder: configure =>
            {
                configure.HasName("Serilog.Core.Logger");
            });

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.Single(plugins);
            Assert.Equal("Serilog.Core.Logger", plugins[0].Name);
            Assert.StartsWith("2.9.0", plugins[0].Version.ToString());
            AssertAssemblyFrameWork(".NETStandard,Version=v2.0", catalog.Single().Assembly);
        }

        [Fact]
        public async Task CanTag()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", configureFinder: configure =>
            {
                configure.HasName("Serilog.Core.Logger")
                    .Tag("CustomTag");
            });

            // Act
            await catalog.Initialize();
            var plugin = catalog.Single();

            // Assert
            Assert.Equal("CustomTag", plugin.Tag);
        }

        [Fact]
        public async Task CanConfigureNamingOptions()
        {
            var options = new NugetPluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified" }
            };

            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", configureFinder: configure =>
            {
                configure.HasName("Serilog.Core.Logger");
            }, options: options);

            // Act
            await catalog.Initialize();
            var plugin = catalog.Single();

            // Assert
            Assert.EndsWith("Modified", plugin.Name);
        }

        [Fact]
        public async Task InstallPackageWithDepencencies()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Moq", "4.13.1");

            // Act
            await catalog.Initialize();
            var pluginAssemblies = catalog.GetPlugins().GroupBy(x => x.Assembly).ToList();

            // Assert
            Assert.Single(pluginAssemblies);
            Assert.EndsWith("Moq.dll", pluginAssemblies.Single().Key.Location);
            Assert.StartsWith("4.13.1", catalog.GetPlugins().First().Version.ToString());
            AssertAssemblyFrameWork(".NETStandard,Version=v2.0", pluginAssemblies.Single().Key);
        }

        [Fact]
        public async Task InstallPackageWithVersionWildcard()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Polly", "7.0.*");

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.NotEmpty(plugins);
        }

        [Fact]
        public async Task InstallPreReleasePackageWithVersionWildcard()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.1-dev*", includePrerelease: true, configureFinder: configure =>
            {
                configure.HasName("Serilog.Core.Logger");
            });

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.Single(plugins);
            Assert.Equal("Serilog.Core.Logger", plugins[0].Name);
            Assert.StartsWith("2.9.1", plugins[0].Version.ToString());
            Assert.StartsWith("2.9.1-dev", plugins[0].ProductVersion);
            AssertAssemblyFrameWork(".NETStandard,Version=v2.0", catalog.Single().Assembly);
        }

        [Fact]
        public async Task InstallPackageFromFeed()
        {
            // Arrange
            var feed = new NuGetFeed("nuget.org", "https://api.nuget.org/v3/index.json");
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", packageFeed: feed);

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.NotEmpty(plugins);
        }

        [Fact]
        public async Task InstallPackageFromFeedUsingFeedName()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", packageFeed: new NuGetFeed("nuget.org"));

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.NotEmpty(plugins);
        }

        [Fact]
        public async Task InstallPackageFromFeedUsingCustomNuGetConfig()
        {
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", packageFeed: new NuGetFeed("nuget.org_test"),
                packagesFolder: _packagesFolderInTestsBin);

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.NotEmpty(plugins);
        }
        
        [Collection(nameof(NotThreadSafeResourceCollection))]
        public class DefaultOptions : IDisposable
        {
            public DefaultOptions()
            {
                NugetPluginCatalogOptions.Defaults.PluginNameOptions =
                    new PluginNameOptions() { PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified" };
            }
            
            [Fact]
            public async Task CanConfigureDefaultNamingOptions()
            {
                // Arrange
                var catalog = new NugetPackagePluginCatalog("Serilog", "2.9.0", configureFinder: configure =>
                {
                    configure.HasName("Serilog.Core.Logger");
                });

                // Act
                await catalog.Initialize();
                var plugin = catalog.Single();

                // Assert
                Assert.EndsWith("Modified", plugin.Name);
            }
            
            public void Dispose()
            {
                NugetPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions();
            }
        }
        
        [Fact]
        public async Task CanInstallPackageWithNativeDepencencies()
        {
            var options = new NugetPluginCatalogOptions()
            {
                TypeFinderOptions = new TypeFinderOptions()
                {
                    TypeFinderCriterias = new List<TypeFinderCriteria>()
                    {
                        new TypeFinderCriteria()
                        {
                            Query = (context, type) =>
                            {
                                if (string.Equals(type.Name, "SqlConnection"))
                                {
                                    return true;
                                }

                                return false;
                            }
                        }
                    }
                }
            };
            // Arrange
            var catalog = new NugetPackagePluginCatalog("Microsoft.Data.SqlClient", "2.1.2", options: options);

            // Act
            await catalog.Initialize();

            var plugin = catalog.Single();

            // This is the connection string part of the Weik.io docs. It provides readonly access to the Adventureworks database sample.
            // So it should be ok to have it here.
            dynamic conn = Activator.CreateInstance(plugin, "Server=tcp:adafydevtestdb001.database.windows.net,1433;User ID=docs;Password=3h1@*6PXrldU4F95;Integrated Security=false;Initial Catalog=adafyweikiodevtestdb001;");

            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "select top 1 * from SalesLT.Customer";

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(String.Format("{0}", reader[0]));
            }

            conn.Dispose();
        }
    }
}
