using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;
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
                    TypeFinderOptions = new TypeFinderOptions()
                    {
                        TypeFinderCriterias = new List<TypeFinderCriteria>()
                        {
                            TypeFinderCriteriaBuilder.Create().Tag("MyTag_1"),
                            TypeFinderCriteriaBuilder.Create().Tag("AnotherTag")
                        }
                    }
                });

            await catalog.Initialize();

            var plugin = catalog.Single();
            Assert.Equal("MyTag_1", plugin.Tag);
        }

        [Fact]
        public async Task CanTagAssemblyPlugin()
        {
            var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\TestAssembly1.dll".Replace(@"\",Path.DirectorySeparatorChar.ToString()), null,
                taggedFilters: new Dictionary<string, Predicate<Type>>() { { "CustomTag", type => true } });

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                Assert.Equal("CustomTag", plugin.Tag);
            }
        }

        [Fact]
        public async Task CanTagAssemblyPluginUsingBuilder()
        {
            var catalog = new AssemblyPluginCatalog(typeof(TypePlugin).Assembly, builder =>
            {
                builder.AssignableTo(typeof(TypePlugin))
                    .Tag("operator");
            });
            
            await catalog.Initialize();
            
            var allPlugins = catalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                Assert.Equal("operator", plugin.Tag);
            }
        }
        
        [Fact]
        public async Task CanTagFolderPlugin()
        {
            var _pluginFolder = @"..\..\..\..\..\Assemblies\bin\netstandard2.0".Replace(@"\",Path.DirectorySeparatorChar.ToString());
            var catalog = new FolderPluginCatalog(_pluginFolder, builder =>
            {
                builder.Tag("test_folder_tag");
            });
            
            await catalog.Initialize();
            
            var allPlugins = catalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                Assert.Equal("test_folder_tag", plugin.Tag);
            }
        }
        
        [Fact]
        public async Task CanTagDelegate()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, bool>(i => true), options: new DelegatePluginCatalogOptions()
            {
                Tags = new List<string>(){"CustomTagDelegate"}
            });

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            foreach (var plugin in allPlugins)
            {
                Assert.Equal("CustomTagDelegate", plugin.Tag);
            }
        }
                
        [Fact]
        public async Task PluginCanContainManyTags()
        {
            var catalog = new TypePluginCatalog(typeof(TypePlugin),
                new TypePluginCatalogOptions()
                {
                    TypeFinderOptions = new TypeFinderOptions()
                    {
                        TypeFinderCriterias = new List<TypeFinderCriteria>()
                        {
                            TypeFinderCriteriaBuilder.Create().Tag("MyTag_1"),
                            TypeFinderCriteriaBuilder.Create().Tag("AnotherTag")
                        }
                    }
                });

            await catalog.Initialize();

            var plugin = catalog.Single();
            
            var coll = new List<string>(){"MyTag_1", "AnotherTag"};
            Assert.Equal(coll, plugin.Tags);
        }
        
        [Collection(nameof(NotThreadSafeResourceCollection))]
        public class DefaultOptions : IDisposable
        {
            public DefaultOptions()
            {
                TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().Tag("CustomTag"));
                TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().HasName(nameof(TypePlugin)).Tag("MyTag_1"));
                TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().HasName("*Json*").Tag("MyTag_1"));
            }

            [Fact]
            public async Task CanTagUsingDefaultOptions()
            {
                var assemblyPluginCatalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\netstandard2.0\TestAssembly1.dll".Replace(@"\",Path.DirectorySeparatorChar.ToString()));
                var typePluginCatalog = new TypePluginCatalog(typeof(TypePlugin));

                var compositeCatalog = new CompositePluginCatalog(assemblyPluginCatalog, typePluginCatalog);

                await compositeCatalog.Initialize();

                var customTaggedPlugins = compositeCatalog.GetByTag("CustomTag");
                Assert.Equal(2, customTaggedPlugins.Count);

                var myTaggedPlugins = compositeCatalog.GetByTag("MyTag_1");
                Assert.Single(myTaggedPlugins);
                
                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
            }
            
            [Fact]
            public async Task TypeCatalogCanTagUsingDefaultOptions()
            {
                var typePluginCatalog = new TypePluginCatalog(typeof(TypePlugin));

                await typePluginCatalog.Initialize();

                var myTaggedPlugins = typePluginCatalog.GetByTag("MyTag_1");
                Assert.Single(myTaggedPlugins);
                
                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
            }

            [Fact]
            public async Task DefaultTagsWithFolderCatalogTypeShouldNotDuplicatePlugins()
            {
                var catalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0".Replace(@"\",Path.DirectorySeparatorChar.ToString()));
                await catalog.Initialize();

                Assert.Single(catalog.GetPlugins());
                var plugin = catalog.Get();
                
                Assert.Equal(2, plugin.Tags.Count);
                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
            }
            
            [Fact]
            public async Task DefaultTagsWithAssemblyCatalogTypeShouldNotDuplicatePlugins()
            {
                var catalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0\JsonNetNew.dll".Replace(@"\",Path.DirectorySeparatorChar.ToString()));
                await catalog.Initialize();

                Assert.Single(catalog.GetPlugins());
                var plugin = catalog.Get();
                
                Assert.Equal(2, plugin.Tags.Count);
                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
            }
            
            public void Dispose()
            {
                TypeFinderOptions.Defaults.TypeFinderCriterias.Clear();
            }
        }
    }
}
