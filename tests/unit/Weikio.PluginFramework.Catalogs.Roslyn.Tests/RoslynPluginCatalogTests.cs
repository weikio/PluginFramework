using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Xunit;
using IPluginCatalogExtensions = Weikio.PluginFramework.Abstractions.IPluginCatalogExtensions;

namespace Weikio.PluginFramework.Catalogs.Roslyn.Tests
{
    public class RoslynPluginCatalogTests
    {
        [Fact]
        public async Task ThrowsWithInvalidCode()
        {
            // Arrange
            var invalidCode = @"public class MyClass
                   {
                       public void RunThings // <- missing parentheses
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            // Assert
            await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CreateCatalog(invalidCode));
        }

        [Fact]
        public async Task ThrowsWithEmptyScript()
        {
            // Arrange
            var invalidCode = "";

            // Assert: Throws
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CreateCatalog(invalidCode));
        }

        [Fact]
        public async Task CanHandleScript()
        {
            // Arrange
            var code = "Debug.WriteLine(\"Hello world!\");";

            // Act
            var types = await TestHelpers.CreateCatalog(code);

            // Assert
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            Assert.Equal(typeof(Task), method.ReturnParameter.ParameterType);
        }

        [Fact]
        public async Task ScriptAssemblyContainsValidVersion()
        {
            // Arrange
            var code = "Debug.WriteLine(\"Hello world!\");";

            // Act
            var type = (await TestHelpers.CreateCatalog(code)).Single();

            // Assert
            var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
            var fileVersion = versionInfo.FileVersion;
            Assert.NotNull(fileVersion);
        }

        [Fact]
        public async Task ScriptContainsVersion1000ByDefault()
        {
            // Arrange
            var code = "Debug.WriteLine(\"Hello world!\");";

            // Act
            var type = (await TestHelpers.CreateCatalog(code)).Single();

            // Assert
            var versionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
            var fileVersion = versionInfo.FileVersion;
            Assert.Equal("1.0.0.0", fileVersion);
        }

        [Fact]
        public async Task CanHandleRegular()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            // Act
            var types = await TestHelpers.CompileRegular(code);

            // Assert
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            Assert.Equal(typeof(void), method.ReturnParameter.ParameterType);
        }

        [Fact]
        public async Task CanCreatePluginWithoutName()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var catalog = new RoslynPluginCatalog(code);

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();
            var firstPluginName = plugins.First().Name;

            // Assert
            Assert.NotEmpty(firstPluginName);
        }

        [Fact]
        public async Task CanCreatePluginWithName()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions() { PluginName = "MyPlugin", };

            // Act
            var catalog = new RoslynPluginCatalog(code, options);
            await catalog.Initialize();

            var plugin = catalog.Get("MyPlugin", new Version(1, 0, 0, 0));

            // Assert
            Assert.NotNull(plugin);
        }

        [Fact]
        public async Task PluginDefaultsToVersion100()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions() { PluginName = "MyPlugin", };

            var catalog = new RoslynPluginCatalog(code, options);

            // Act
            await catalog.Initialize();
            var plugin = catalog.Get("MyPlugin", new Version(1, 0, 0, 0));

            // Assert
            Assert.Equal(new Version(1, 0, 0, 0), plugin.Version);
        }

        [Fact]
        public async Task CanCreatePluginWithNameAndVersion()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions() { PluginName = "MyPlugin", PluginVersion = new Version(1, 1) };

            var catalog = new RoslynPluginCatalog(code, options);

            // Act
            await catalog.Initialize();
            var plugin = catalog.Get("MyPlugin", new Version(1, 1));

            // Assert
            Assert.NotNull(plugin);
        }

        [Fact]
        public async Task CanGetAllFromCatalog()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var catalog = new RoslynPluginCatalog(code);

            // Act
            await catalog.Initialize();
            var plugins = catalog.GetPlugins();
            var plugin = plugins.FirstOrDefault();

            // Assert
            Assert.NotNull(plugin);
        }

        [Fact]
        public async Task CanCreatePluginNameWithGenerator()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginNameGenerator = (nameOptions, type) => "HelloThereFromGenerator" },
            };

            var catalog = new RoslynPluginCatalog(code, options);

            // Act
            await catalog.Initialize();

            // Assert
            var plugin = catalog.Get("HelloThereFromGenerator", Version.Parse("1.0.0.0"));
            Assert.NotNull(plugin);
        }

        [Fact]
        public async Task CanCreatePluginVersionWithGenerator()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions()
            {
                PluginNameOptions = new PluginNameOptions() { PluginVersionGenerator = (nameOptions, type) => new Version(2, 0, 0) }
            };

            var catalog = new RoslynPluginCatalog(code, options);

            // Act
            await catalog.Initialize();
            var plugin = catalog.Single();

            // Assert
            Assert.Equal(new Version(2, 0, 0), plugin.Version);
        }

        [Fact]
        public async Task CanTagCode()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                           var y = 0;
                           var a = 1;
           
                           a = y + 10;
                       
                           Debug.WriteLine(y + a);
                       }
                   }";

            var catalog = new RoslynPluginCatalog(code, new RoslynPluginCatalogOptions() { Tags = new List<string>() { "CustomTag" } });

            await catalog.Initialize();
            var plugin = catalog.Single();

            Assert.Equal("CustomTag", plugin.Tag);
        }

        [Fact]
        public async Task CanTagScript()
        {
            // Arrange
            var code = "Debug.WriteLine(\"Hello world!\");";

            var catalog = new RoslynPluginCatalog(code, new RoslynPluginCatalogOptions() { Tags = new List<string>() { "CustomTag" } });

            await catalog.Initialize();

            var plugin = catalog.Single();

            Assert.Equal("CustomTag", plugin.Tag);
        }
    }
}
