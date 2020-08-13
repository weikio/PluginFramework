using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Tests.Plugins;
using Weikio.PluginFramework.TypeFinding;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class TagTests
    {
        [Fact]
        public async Task CanTagTypePlugin()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin),
                new TypePluginCatalogOptions()
                {
                    TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>()
                    {
                        { "MyTag_1", new TypeFinderCriteria() { Query = (context, type) => true } }
                    }
                });
            
            await catalog.Initialize();

            var plugin = catalog.Single();
            Assert.Equal("MyTag_1", plugin.Tag);
        }

        [Fact]
        public async Task CanTagAssemblyPlugin()
        {
            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\TestAssembly1.dll", null,
                taggedFilters: new Dictionary<string, Predicate<Type>>() { { "CustomTag", type => true } });

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                Assert.Equal("CustomTag", plugin.Tag);
            }
        }
    }
}
