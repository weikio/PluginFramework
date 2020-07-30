using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Scripting;

namespace Weikio.PluginFramework.Catalogs.Roslyn
{
    /// <summary>
    /// Initializer which can handle C# script
    /// </summary>
    public class ScriptInitializer
    {
        private readonly string _code;
        private readonly RoslynPluginCatalogOptions _options;

        public ScriptInitializer(string code, RoslynPluginCatalogOptions options)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentOutOfRangeException(nameof(code), code, "Script can not be null or empty");
            }

            _code = code;
            _options = options;

            _options = options ?? new RoslynPluginCatalogOptions();

            if (_options.TypeNameGenerator is null)
            {
                throw new ArgumentNullException(nameof(_options.TypeNameGenerator));
            }

            if (_options.NamespaceNameGenerator is null)
            {
                throw new ArgumentNullException(nameof(_options.NamespaceNameGenerator));
            }
            
            if (_options.MethodNameGenerator is null)
            {
                throw new ArgumentNullException(nameof(_options.MethodNameGenerator));
            }
        }

        public async Task<Assembly> CreateAssembly()
        {
            try
            {
                var returnType = await GetReturnType();
                var parameters = await GetParameters();
                var updatedScript = await RemoveProperties(_code);

                var generator = new AssemblyGenerator();
                generator.ReferenceAssemblyContainingType<Action>();
                string assemblySourceCode;

                using (var sourceWriter = new SourceWriter())
                {
                    sourceWriter.UsingNamespace("System");
                    sourceWriter.UsingNamespace("System.Diagnostics");
                    sourceWriter.UsingNamespace("System.Threading.Tasks");
                    sourceWriter.UsingNamespace("System.Text");
                    sourceWriter.UsingNamespace("System.Collections");
                    sourceWriter.UsingNamespace("System.Collections.Generic");

                    sourceWriter.Namespace(_options.NamespaceNameGenerator(_options));
                    sourceWriter.StartClass(_options.TypeNameGenerator(_options));

                    if (returnType == null)
                    {
                        if (_options.ReturnsTask)
                        {
                            sourceWriter.Write($"BLOCK:public async Task {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                        }
                        else
                        {
                            sourceWriter.Write($"BLOCK:public void {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                        }
                    }
                    else
                    {
                        if (_options.ReturnsTask)
                        {
                            sourceWriter.Write($"BLOCK:public async Task<{returnType.FullName}> {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                        }
                        else
                        {
                            sourceWriter.Write($"BLOCK:public {returnType.FullName} {_options.MethodNameGenerator(_options)}({GetParametersString(parameters)})");
                        }
                    }

                    sourceWriter.Write(updatedScript);

                    sourceWriter.FinishBlock(); // Finish the method
                    sourceWriter.FinishBlock(); // Finish the class
                    sourceWriter.FinishBlock(); // Finish the namespace

                    assemblySourceCode = sourceWriter.Code();
                }

                var result = generator.Generate(assemblySourceCode);

                return result;
            }
            catch (Exception e)
            {
                throw new InvalidCodeException("Failed to create assembly from script. Code: " + _code, e);
            }
        }

        private async Task<string> RemoveProperties(string currentScript)
        {
            var tree = CSharpSyntaxTree.ParseText(currentScript);
            var root = await tree.GetRootAsync();

            var descendants = root
                .DescendantNodes().ToList();

            var declarations = descendants.OfType<PropertyDeclarationSyntax>().ToList();

            if (declarations?.Any() != true)
            {
                return currentScript;
            }

            var firstProperty = declarations.First();
            root = root.RemoveNode(firstProperty, SyntaxRemoveOptions.KeepEndOfLine);

            var updatedScript = root.GetText();
            var result = updatedScript.ToString();

            return await RemoveProperties(result);
        }

        private async Task<List<(string, Type)>> GetParameters()
        {
            var csharScript = CSharpScript.Create(_code, ScriptOptions.Default);

            var compilation = csharScript.GetCompilation();

            var syntaxTree = compilation.SyntaxTrees.Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var descendants = (await syntaxTree.GetRootAsync())
                .DescendantNodes().ToList();

            var declarations = descendants.OfType<PropertyDeclarationSyntax>().ToList();
            var result = new List<(string, Type)>();

            if (declarations?.Any() != true)
            {
                return result;
            }

            var symbolDisplayFormat =
                new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            foreach (var propertyDeclaration in declarations)
            {
                var name = propertyDeclaration.Identifier.Text;
                var typeInfo = semanticModel.GetTypeInfo(propertyDeclaration.Type);

                var typeName = typeInfo.Type.ToDisplayString(symbolDisplayFormat);
                var type = Type.GetType(typeName, true);

                result.Add((name, type));
            }

            return result;
        }

        private async Task<Type> GetReturnType()
        {
            var csharScript = CSharpScript.Create(_code, ScriptOptions.Default);

            var compilation = csharScript.GetCompilation();

            var syntaxTree = compilation.SyntaxTrees.Single();
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var descendants = (await syntaxTree.GetRootAsync())
                .DescendantNodes().ToList();

            var returnSyntax = descendants.OfType<ReturnStatementSyntax>().FirstOrDefault();

            if (returnSyntax == null)
            {
                return null;
            }

            var expr = returnSyntax.Expression;

            var typeInfo = semanticModel.GetTypeInfo(expr);

            var symbolDisplayFormat =
                new SymbolDisplayFormat(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

            string fullyQualifiedName;

            if (typeInfo.Type is INamedTypeSymbol mySymbol)
            {
                fullyQualifiedName = mySymbol.ToDisplayString(symbolDisplayFormat);
            }
            else
            {
                fullyQualifiedName = typeInfo.Type.ToDisplayString(symbolDisplayFormat);
            }

            try
            {
                var result = Type.GetType(fullyQualifiedName, true);

                return result;
            }
            catch (Exception e)
            {
                throw new NotSupportedException($"{fullyQualifiedName} is not supported return type of a script.", e);
            }
        }

        private string GetParametersString(List<(string, Type)> parameters)
        {
            if (parameters?.Any() != true)
            {
                return "";
            }

            var result = string.Join(", ", parameters.Select(x => $"{x.Item2.FullName} {x.Item1}"));

            return result;
        }
    }
}
