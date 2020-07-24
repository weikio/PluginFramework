using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    /// <summary>
    ///     Note: Heavily based on the work done by Jeremy Miller in Lamar: https://github.com/jasperfx/Lamar
    /// </summary>
    public class CodeToAssemblyGenerator
    {
        private readonly IList<Assembly> _assemblies = new List<Assembly>();
        private readonly IList<MetadataReference> _references = new List<MetadataReference>();

        public CodeToAssemblyGenerator()
        {
            ReferenceAssemblyContainingType<object>();
            ReferenceAssembly(typeof(Enumerable).GetTypeInfo().Assembly);
        }

        public string AssemblyName { get; set; }

        public void ReferenceAssembly(Assembly assembly)
        {
            if (assembly == null || _assemblies.Contains(assembly))
            {
                return;
            }

            _assemblies.Add(assembly);

            try
            {
                var referencePath = CreateAssemblyReference(assembly);

                if (referencePath == null)
                {
                    Console.WriteLine("Could not make an assembly reference to " + assembly.FullName);
                }
                else
                {
                    if (_references.Any(
                        x => x.Display == referencePath))
                    {
                        return;
                    }

                    _references.Add(MetadataReference.CreateFromFile(referencePath));

                    foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
                    {
                        ReferenceAssembly(Assembly.Load(referencedAssembly));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not make an assembly reference to {assembly.FullName}\n\n{ex}");
            }
        }

        private static string CreateAssemblyReference(Assembly assembly)
        {
            if (assembly.IsDynamic)
            {
                return null;
            }

            return assembly.Location;
        }

        public void ReferenceAssemblyContainingType<T>()
        {
            ReferenceAssembly(typeof(T).GetTypeInfo().Assembly);
        }

        public Assembly Generate(string code)
        {
            var str = AssemblyName ?? Path.GetRandomFileName();
            var text = CSharpSyntaxTree.ParseText(code);
            var array = _references.ToArray();
            var syntaxTreeArray = new SyntaxTree[1] { text };

            using (var memoryStream = new MemoryStream())
            {
                var emitResult = CSharpCompilation
                    .Create(str, syntaxTreeArray, array,
                        new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, false, null,
                            null, null, null, OptimizationLevel.Debug, false,
                            false, null, null, new ImmutableArray<byte>(), new bool?())).Emit(memoryStream);

                if (!emitResult.Success)
                {
                    var errors = emitResult.Diagnostics
                        .Where(diagnostic =>
                            diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

                    var errorsMsg = string.Join("\n", errors.Select(x => x.Id + ": " + x.GetMessage()));

                    var errorMsgWithCode = "Compilation failures!" + Environment.NewLine + Environment.NewLine +
                                           errorsMsg + Environment.NewLine + Environment.NewLine + "Code:" +
                                           Environment.NewLine + Environment.NewLine + code;

                    throw new InvalidOperationException(errorMsgWithCode);
                }

                memoryStream.Seek(0L, SeekOrigin.Begin);

                var c = new CustomAssemblyLoadContext();

                var assembly = c.LoadFromStream(memoryStream);

                return assembly;
            }
        }
    }
}