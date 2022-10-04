using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Weikio.NugetDownloader;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.NuGet;
using Xunit;

namespace PluginFramework.Catalogs.NuGet.Tests
{
    public class NugetFeedPluginCatalogTests
    {
        // this folder is defined in test project's bin so that NuGet implementation
        // will use the NuGet.Config defined in the test project root.
        private readonly string _packagesFolderInTestsBin;

        public NugetFeedPluginCatalogTests()
        {
            var executingAssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _packagesFolderInTestsBin = Path.Combine(executingAssemblyDir, "FeedTestPackages");
        }

        private void AssertAssemblyFrameWork(string targetFramework, Assembly assembly)
        {
            var assemblyFramework = assembly
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            Assert.Equal(targetFramework, assemblyFramework);
        }

        [Fact]
        public async Task InstallUsingTagFilter()
        {
            // Arrange
            var feed = new NuGetFeed("nuget.org", "https://api.nuget.org/v3/index.json");
            var catalog = new NugetFeedPluginCatalog(feed, searchTerm: "tags:mocking", maxPackages: 1, configureFinder: configure =>
            {
                configure.HasName("Moq.Range");
            });

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();

            // Assert
            Assert.Single(plugins);
            Assert.Equal("Moq.Range", plugins[0].Name);
            Assert.StartsWith("4.", plugins[0].Version.ToString());
            AssertAssemblyFrameWork(".NETStandard,Version=v2.1", plugins.Single().Assembly);
        }
        
        [Fact]
        public async Task CanTag()
        {
            // Arrange
            var feed = new NuGetFeed("nuget.org", "https://api.nuget.org/v3/index.json");
            var catalog = new NugetFeedPluginCatalog(feed, searchTerm: "tags:mocking", maxPackages: 1, configureFinder: configure =>
            {
                configure.HasName("Moq.Range")
                    .Tag("MockSolutions");
            });

            // Act
            await catalog.Initialize();
            var plugin = catalog.Single();

            // Assert
            Assert.Equal("MockSolutions", plugin.Tag);
        }
        
        [Fact]
        public async Task CanConfigureNamingOptions()
        {
            var options = new NugetFeedPluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified" }
            };

            // Arrange
            var feed = new NuGetFeed("nuget.org", "https://api.nuget.org/v3/index.json");
            var catalog = new NugetFeedPluginCatalog(feed, searchTerm: "tags:mocking", maxPackages: 1, configureFinder: configure =>
            {
                configure.HasName("Moq.Range");
            }, options: options);

            // Act
            await catalog.Initialize();
            var plugin = catalog.Single();

            // Assert
            Assert.EndsWith("Modified", plugin.Name);
        }
 
        [Collection(nameof(NotThreadSafeResourceCollection))]
        public class DefaultOptions : IDisposable
        {
            public DefaultOptions()
            {
                NugetFeedPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
                {
                    PluginNameGenerator = (nameOptions, type) => type.FullName + "Modified"
                };
            }
            
            [Fact]
            public async Task CanConfigureDefaultNamingOptions()
            {

                // Arrange
                var feed = new NuGetFeed("nuget.org", "https://api.nuget.org/v3/index.json");
                var catalog = new NugetFeedPluginCatalog(feed, searchTerm: "tags:mocking", maxPackages: 1, configureFinder: configure =>
                {
                    configure.HasName("Moq.Range");
                });

                // Act
                await catalog.Initialize();
                var plugin = catalog.Single();

                // Assert
                Assert.EndsWith("Modified", plugin.Name);
            } 
            
            public void Dispose()
            {
                NugetFeedPluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions();
            }
        }
    }
}
