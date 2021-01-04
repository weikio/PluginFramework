using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
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

            var plugins = catalog.GetPlugins();
            Assert.Single(plugins);
        }

        [Fact]
        public async Task NameIsTypeFullName()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin));
            await catalog.Initialize();

            var thePlugin = catalog.Single();
        
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

            var thePlugin = catalog.Single();
        
            Assert.Equal("HelloOptions", thePlugin.Name);
        }
        
        
        [Fact]
        public async Task CanSetNameByAttribute()
        {
            var catalog = new TypePluginCatalog(typeof(TypePluginWithName));
            await catalog.Initialize();

            var thePlugin = catalog.Single();
        
            Assert.Equal("MyCustomName", thePlugin.Name);
        }
        
        
        [Fact]
        public async Task CanConfigureNamingOptions()
        {
            var options = new TypePluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (opt, type) => "HelloOptions" }
            };
            
            var catalog = new TypePluginCatalog(typeof(TypePlugin), options);
        
            await catalog.Initialize();

            var thePlugin = catalog.Single();
        
            Assert.Equal("HelloOptions", thePlugin.Name);
        }
        
        [Fact]
        public async Task CanConfigureDefaultNamingOptions()
        {
            TypePluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
            {
                PluginNameGenerator = (nameOptions, type) => "HelloOptions"
            };
            
            var catalog = new TypePluginCatalog(typeof(TypePlugin));
        
            await catalog.Initialize();

            var thePlugin = catalog.Single();
        
            Assert.Equal("HelloOptions", thePlugin.Name);
        }
        
        [Fact]
        public async Task CanOverrideDefaultNamingOptions()
        {
            var options = new TypePluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (opt, type) => "Overridden" }
            };
            
            TypePluginCatalogOptions.Defaults.PluginNameOptions = new PluginNameOptions()
            {
                PluginNameGenerator = (nameOptions, type) => "HelloOptions"
            };
            
            var catalog = new TypePluginCatalog(typeof(TypePlugin));
            var catalog2 = new TypePluginCatalog(typeof(TypePlugin), options);

            await catalog.Initialize();
            await catalog2.Initialize();

            var thePlugin = catalog.Single();
            Assert.Equal("HelloOptions", thePlugin.Name);

            var anotherPlugin = catalog2.Single();
            Assert.Equal("Overridden", anotherPlugin.Name);
        }
    }
}
