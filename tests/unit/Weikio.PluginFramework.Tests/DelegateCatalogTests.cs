using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs.Delegates;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.PluginFramework.Tests
{
    public class DelegateCatalogTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DelegateCatalogTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task CanInitialize()
        {
            var catalog = new DelegatePluginCatalog(new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            }));

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            Assert.NotEmpty(allPlugins);
        }

        [Fact]
        public async Task CanInitializeFunc()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, bool>(i => true));

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            Assert.NotEmpty(allPlugins);
        }

        [Fact]
        public async Task CanInitializeAsyncAction()
        {
            var catalog = new DelegatePluginCatalog(new Action<int>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");
            }));

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            Assert.NotEmpty(allPlugins);
        }

        [Fact]
        public async Task CanInitializeAsyncFunc()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var allPlugins = catalog.GetPlugins();

            Assert.NotEmpty(allPlugins);
        }

        [Fact]
        public async Task ByDefaultNoProperties()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Empty(pluginType.GetProperties());
        }
        
        [Fact]
        public async Task ByDefaultRunMethod()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            var methodInfos = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assert.Single(methodInfos);
            Assert.Equal("Run", methodInfos.Single().Name);
        }
        
        [Fact]
        public async Task ByDefaultGeneratedNamespace()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("GeneratedNamespace", pluginType.Namespace);
        }
        
        [Fact]
        public async Task CanConfigurePluginNameFromConstructor()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), "HelloPlugin");

            await catalog.Initialize();

            Assert.Equal("HelloPlugin", catalog.Single().Name);
        }
        
        [Fact]
        public async Task CanConfigureNamespace()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), new DelegatePluginCatalogOptions()
            {
                NamespaceName = "HelloThereNs"
            });

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("HelloThereNs", pluginType.Namespace);
        }
        
        [Fact]
        public async Task CanConfigureNamespaceUsinGenerator()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), new DelegatePluginCatalogOptions()
                {
                    NamespaceNameGenerator = options => "GeneratorNS"
                }
           );

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("GeneratorNS", pluginType.Namespace);
        }
        
        [Fact]
        public async Task ByDefaultGeneratedTypeName()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("GeneratedType", pluginType.Name);
        }
        
        [Fact]
        public async Task CanConfigureTypename()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), new DelegatePluginCatalogOptions()
            {
                TypeName = "HelloThereType"
            });

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("HelloThereType", pluginType.Name);
        }
        
        [Fact]
        public async Task CanConfigureTypenameUsinGenerator()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    _testOutputHelper.WriteLine("Hello from test");

                    return true;
                }), new DelegatePluginCatalogOptions()
                {
                    TypeNameGenerator = options => "GeneratorTypeName"
                }
            );

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Equal("GeneratorTypeName", pluginType.Name);
        }
        
        [Fact]
        public async Task CanConfigureGeneratedMethodName()
        {
            var options = new DelegatePluginCatalogOptions()
            {
                MethodName = "HelloMethod"
            };
            
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), options);

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            var methodInfos = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assert.Equal("HelloMethod", methodInfos.Single().Name);
        }
        
        [Fact]
        public async Task CanConfigureGeneratedMethodNameUsingGenerator()
        {
            var options = new DelegatePluginCatalogOptions()
            {
                MethodNameGenerator = catalogOptions => "MethodGeneratorName"
            };
            
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), options);

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            var methodInfos = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assert.Equal("MethodGeneratorName", methodInfos.Single().Name);
        }

        [Fact]
        public async Task ByDefaultNoConstructorParameters()
        {
            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }));

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Single(pluginType.GetConstructors());

            foreach (var constructorInfo in pluginType.GetConstructors())
            {
                Assert.Empty(constructorInfo.GetParameters());
            }
        }

        [Fact]
        public async Task CanConvertParameterToProperty()
        {
            var rules = new List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)>
            {
                (info => info.ParameterType == typeof(int), nfo => new ParameterConversion() { ToPublicProperty = true })
            };

            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), rules);

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Single(pluginType.GetProperties());
        }
        
        [Fact]
        public async Task CanConvertParameterToConstructorParameter()
        {
            var rules = new List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)>
            {
                (info => info.ParameterType == typeof(int), nfo => new ParameterConversion() { ToConstructor = true })
            };

            var catalog = new DelegatePluginCatalog(new Func<int, Task<bool>>(async i =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), rules);

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Single(pluginType.GetConstructors());
            Assert.Single(pluginType.GetConstructors().Single().GetParameters());
        }
        
        [Fact]
        public async Task CanConvertMultipleParametersToConstructorAndPropertyParameters()
        {
            var rules = new List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)>
            {
                (info => info.ParameterType == typeof(int), nfo => new ParameterConversion() { ToConstructor = true }),
                (info => info.ParameterType == typeof(string), nfo => new ParameterConversion() { ToPublicProperty = true }),
                (info => info.ParameterType == typeof(bool), nfo => new ParameterConversion() { ToPublicProperty = true }),
                (info => info.ParameterType == typeof(decimal), nfo => new ParameterConversion() { ToConstructor = true }),
            };

            var catalog = new DelegatePluginCatalog(new Func<int, string, bool, decimal, char, bool>( (i, s, arg3, arg4, c) => 
            {
                _testOutputHelper.WriteLine("Hello from test");

                return true;
            }), rules);

            await catalog.Initialize();

            var pluginType = catalog.Single().Type;
            Assert.Single(pluginType.GetConstructors());
            Assert.Equal(2, pluginType.GetConstructors().Single().GetParameters().Length);
            
            Assert.Equal(2, pluginType.GetProperties().Length);

            dynamic obj = Activator.CreateInstance(pluginType, new object[]{30, new decimal(22)});
            obj.S = "hello";
            obj.Arg3 = true;

            var res = obj.Run('g');
            Assert.True(res);
        }
        
        [Fact]
        public async Task CanSetPluginName()
        {
            var catalog = new DelegatePluginCatalog(new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            }), nameOptions: new PluginNameOptions()
            {
                PluginNameGenerator = (options, type) => "CustomPlugin"
            });

            await catalog.Initialize();

            var plugin = catalog.Single();
            Assert.Equal("CustomPlugin", plugin.Name);
        }
        
        [Fact]
        public async Task CanSetPluginNameAndVersion()
        {
            var catalog = new DelegatePluginCatalog(new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            }), nameOptions: new PluginNameOptions()
            {
                PluginNameGenerator = (options, type) => "CustomPlugin",
                PluginVersionGenerator = (options, type) => new Version(2, 3, 5)
            });

            await catalog.Initialize();

            var plugin = catalog.Single();
            Assert.Equal("CustomPlugin", plugin.Name);
            Assert.Equal(new Version(2, 3, 5), plugin.Version);
        }
        
        [Fact]
        public async Task CanConfigurePluginsHaveUniqueNames()
        {
            var randomGenerator = new Random(Guid.NewGuid().GetHashCode());
            var nameOptions = new PluginNameOptions() { PluginNameGenerator = (options, type) => $"CustomPlugin{randomGenerator.Next(int.MaxValue)}" };
            
            var catalog = new DelegatePluginCatalog(new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            }), nameOptions: nameOptions);

            var catalog2 = new DelegatePluginCatalog(new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello from test");
            }), nameOptions: nameOptions);
            
            await catalog.Initialize();
            await catalog2.Initialize();

            var firstPlugin = catalog.Single();
            var secondPlugin = catalog2.Single();
            
            Assert.NotEqual(firstPlugin.Name, secondPlugin.Name);
        } 
    }
}
