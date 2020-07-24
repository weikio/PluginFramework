using System.Threading.Tasks;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.PluginFramework.Tests
{
    public class DelegatCatalogTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DelegatCatalogTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task CanInitialize()
        {
            var catalog = new DelegatePluginCatalog(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            });
            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            Assert.NotEmpty(allPlugins);
        }
    }
}
