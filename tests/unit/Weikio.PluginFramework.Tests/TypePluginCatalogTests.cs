using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Tests.Plugins;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class TypePluginCatalogTests
    {
        [Fact]
        public async Task CanInitialize()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin));
            await catalog.Initialize();

            var allPlugins = await catalog.GetPluginsOld();

            Assert.Single(allPlugins);
        }

        [Fact]
        public async Task NameIsTypeFullName()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin));
            await catalog.Initialize();

            var thePlugin = (await catalog.GetPluginsOld()).First();

            Assert.Equal("Weikio.PluginFramework.Tests.Plugins.TypePlugin", thePlugin.Name);
        }

        [Fact]
        public async Task CanConfigureNameResolver()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin), configure =>
            {
                configure.PluginNameGenerator = (opt, type) => "HelloOptions";
            });

            await catalog.Initialize();

            var thePlugin = (await catalog.GetPluginsOld()).First();

            Assert.Equal("HelloOptions", thePlugin.Name);
        }

        [Fact]
        public async Task CanSetNameByAttribute()
        {
            var catalog = new TypePluginCatalog(typeof(TypePluginWithName));
            await catalog.Initialize();

            var thePlugin = (await catalog.GetPluginsOld()).First();

            Assert.Equal("MyCustomName", thePlugin.Name);
        }
    }
}
