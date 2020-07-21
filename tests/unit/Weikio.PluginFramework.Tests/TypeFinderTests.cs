using System.Threading.Tasks;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Tests.Plugins;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class TypeFinderTests
    {
        [Fact]
        public async Task CanGetPluginsByAttribute()
        {
            var catalog = new AssemblyPluginCatalog(typeof(TypeFinderTests).Assembly, configure =>
            {
                configure.HasAttribute(typeof(MyPluginAttribute));
            });

            await catalog.Initialize();
            
            Assert.Equal(2, catalog.GetPlugins().Count);
        }
        
        [Fact]
        public async Task CanGetPluginsByMultipleCriteria()
        {
            var catalog = new AssemblyPluginCatalog(typeof(TypeFinderTests).Assembly, configure =>
            {
                configure.HasAttribute(typeof(MyPluginAttribute))
                    .IsAbstract(true)
                    .HasName(nameof(AbstractPluginWithAttribute));
            });

            await catalog.Initialize();
            
            Assert.Single(catalog.GetPlugins());
        }
    }
}
