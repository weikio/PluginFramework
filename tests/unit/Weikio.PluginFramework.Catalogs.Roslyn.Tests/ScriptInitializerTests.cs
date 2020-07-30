using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Weikio.PluginFramework.Catalogs.Roslyn.Tests
{
    public class ScriptInitializerTests
    {
        [Fact]
        public async Task ThrowsWithInvalidScript()
        {
            // Arrange
            var invalidCode = "not c#";

            // Assert
            await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CompileScript(invalidCode));
        }

        [Fact]
        public async Task ThrowsWithEmptyScript()
        {
            // Arrange
            var invalidCode = "";

            // Assert: Throws
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await TestHelpers.CompileScript(invalidCode));
        }

        [Fact]
        public async Task CanInitVoid()
        {
            // Arrange
            var code = "Debug.WriteLine(\"Hello world!\");";

            // Act
            var types = await TestHelpers.CompileScript(code);

            // Assert
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            Assert.True(method.ReturnParameter.ParameterType == typeof(Task));
        }

        [Fact]
        public async Task CanInitVoidWithParameter()
        {
            // Arrange
            var code = "string dah {get;set;} = \"hello\"; var i = 5; Debug.WriteLine(dah);";

            // Act
            var types = await TestHelpers.CompileScript(code);

            // Assert
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            var methodParameters = method.GetParameters().ToList();
            Assert.Single(methodParameters);
            Assert.True(methodParameters.Single().ParameterType == typeof(string));
        }

        [Fact]
        public async Task CanInitVoidWithMultipleParameters()
        {
            // Arrange
            var code = "int y {get;set;} = 5; int x {get;set;} = 20; x = y + 20; Debug.WriteLine(x);";

            // Act
            var types = await TestHelpers.CompileScript(code);

            // Assert
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            var methodParameters = method.GetParameters().ToList();
            Assert.Equal(2, methodParameters.Count);
            Assert.True(methodParameters.First().ParameterType == typeof(int));
        }

        [Fact]
        public async Task CanInitReturnString()
        {
            // Arrange
            var code = "var i = 15; var x = \"hello\"; return x;";

            // Act
            var types = await TestHelpers.CompileScript(code);

            // Assert: doesn't throw
            Assert.Single(types);

            var method = types.First().GetMethods().First();
            var returnParameter = method.ReturnParameter;

            Assert.NotNull(returnParameter);
            Assert.Equal(typeof(Task<string>), returnParameter.ParameterType);
        }

        [Fact]
        public async Task CanInitReturnStringWithParameters()
        {
            // Arrange
            var code = "int y {get;set;} = 5; int x {get;set;} = 20; x = y + 20; Debug.WriteLine(x); return x.ToString();";

            // Act
            var types = await TestHelpers.CompileScript(code);

            // Assert
            var method = types.First().GetMethods().First();
            var returnParameter = method.ReturnParameter;

            Assert.NotNull(returnParameter);
            Assert.Equal(typeof(Task<string>), returnParameter.ParameterType);
        }

        [Fact]
        public async Task CanNotInitReturnValueTuple()
        {
            var code = "var i = 15; var x = \"hello\"; return (i,x);";

            await Assert.ThrowsAsync<InvalidCodeException>(async () => await TestHelpers.CompileScript(code));
        }
        
        [Fact]
        public async Task ThrowsWithMissingTypeNameGenerator()
        {
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { TypeNameGenerator = null };

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await TestHelpers.CompileScript(code, options));
        }

        [Fact]
        public async Task ThrowsWithMissingNamespaceNameGenerator()
        {
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { NamespaceNameGenerator = null };

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await TestHelpers.CompileScript(code, options));
        }

        [Fact]
        public async Task HasDefaultTypeName()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            // Act
            var type = (await TestHelpers.CompileScript(code)).First();

            // Assert
            var defaultOptions = new RoslynPluginCatalogOptions();

            Assert.Equal(defaultOptions.TypeName, type.Name);
        }

        [Fact]
        public async Task HasDefaultNamespace()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            // Act
            var type = (await TestHelpers.CompileScript(code)).First();

            // Assert
            var defaultOptions = new RoslynPluginCatalogOptions();

            Assert.Equal(defaultOptions.NamespaceName, type.Namespace);
        }

        [Fact]
        public async Task DefaultsToReturningTask()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            // Act
            var type = (await TestHelpers.CompileScript(code)).First();
            var method = type.GetMethods().First();

            // Assert
            Assert.Equal(typeof(Task<string>), method.ReturnParameter.ParameterType);
        }
        
        [Fact]
        public async Task HasDefaultMethodName()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            // Act
            var type = (await TestHelpers.CompileScript(code)).First();
            var method = type.GetMethods().First();

            // Assert
            Assert.Equal("Run", method.Name);
        }
        
        [Fact]
        public async Task CanConfigureMethodName()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { MethodName = "Execute" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();
            var method = type.GetMethods().First();

            // Assert
            Assert.Equal("Execute", method.Name);
        }
        
        [Fact]
        public async Task CanConfigureMethodNameGenerator()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { MethodNameGenerator = catalogOptions => "MyMethod" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();
            var method = type.GetMethods().First();

            // Assert
            Assert.Equal("MyMethod", method.Name);
        }

        [Fact]
        public async Task CanConfigureTypeName()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { TypeName = "MyTest" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();

            // Assert
            Assert.Equal("MyTest", type.Name);
        }

        [Fact]
        public async Task CanConfigureTypeNameGenerator()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            var typeName = "MyTestTypeName";
            var options = new RoslynPluginCatalogOptions() { TypeNameGenerator = catalogOptions => typeName + "15" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();

            // Assert
            Assert.Equal("MyTestTypeName15", type.Name);
        }

        [Fact]
        public async Task CanConfigureNamespace()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { NamespaceName = "MyTestNamespace" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();

            // Assert
            Assert.Equal("MyTestNamespace", type.Namespace);
        }

        [Fact]
        public async Task CanConfigureNamespaceGenerator()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";

            var namespaceName = "MyTestNamespace";
            var options = new RoslynPluginCatalogOptions() { NamespaceNameGenerator = catalogOptions => namespaceName + "10" };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();

            // Assert
            Assert.Equal("MyTestNamespace10", type.Namespace);
        }

        [Fact]
        public async Task CanConfigureReturnsTask()
        {
            // Arrange
            var code = "var x = \"hello\"; return x;";
            var options = new RoslynPluginCatalogOptions() { ReturnsTask = false };

            // Act
            var type = (await TestHelpers.CompileScript(code, options)).First();
            var method = type.GetMethods().First();

            // Assert
            Assert.Equal(typeof(string), method.ReturnParameter.ParameterType);
        }

        [Fact]
        public async Task CanConfigureToHaveUniqueTypeNames()
        {
            // Arrange
            var code = "var i = 15; var x = \"hello\"; return x;";
            var code2 = "var i = 15; var x = \"hello\"; return x;";

            var randomGenerator = new Random(Guid.NewGuid().GetHashCode());

            var options = new RoslynPluginCatalogOptions()
            {
                TypeNameGenerator = catalogOptions =>
                {
                    var defaultTypeName = catalogOptions.TypeName;
                    var result = $"{defaultTypeName}{randomGenerator.Next(int.MaxValue)}";

                    return result;
                }
            };

            // Act
            var types = await TestHelpers.CompileScript(code, options);
            var types2 = await TestHelpers.CompileScript(code2, options);

            // Assert
            var firstType = types.First();
            var secondType = types2.First();

            Assert.NotEqual(firstType.Name, secondType.Name);
        }        
    }
}
