using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Weikio.PluginFramework.Catalogs;
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
            AssertAssemblyFrameWork(".NETStandard,Version=v2.0", plugins.Single().Assembly);
        }
    }
}
