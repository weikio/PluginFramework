using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Weikio.PluginFramework.Catalogs.Roslyn.Tests
{
    public class RegularInitializerTests
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
            await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CompileRegular(invalidCode));
        }

        [Fact]
        public async Task ThrowsWithEmptyScript()
        {
            // Arrange
            var invalidCode = "";

            // Assert: Throws
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CompileRegular(invalidCode));
        }

        [Fact]
        public async Task CanCompileCode()
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

            // Assert: Does not throw
            await TestHelpers.CompileRegular(code);
        }

        [Fact]
        public async Task CanAddReference()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                            Newtonsoft.Json.JsonConvert.SerializeObject(15);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions() { AdditionalReferences = new List<Assembly>() { typeof(Newtonsoft.Json.JsonConvert).Assembly } };

            await TestHelpers.CompileRegular(code, options);
        }

        [Fact]
        public async Task CanAddNamespace()
        {
            // Arrange
            var code = @"public class MyClass
                   {
                       public void RunThings()
                       {
                            JsonConvert.SerializeObject(15);
                       }
                   }";

            var options = new RoslynPluginCatalogOptions()
            {
                AdditionalReferences = new List<Assembly>() { typeof(Newtonsoft.Json.JsonConvert).Assembly },
                AdditionalNamespaces = new List<string>() { "Newtonsoft.Json" }
            };

            await TestHelpers.CompileRegular(code, options);
        }
    }
}
