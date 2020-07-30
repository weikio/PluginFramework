using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LamarCodeGeneration;
using LamarCompiler;

namespace Weikio.PluginFramework.Catalogs.Roslyn
{
    /// <summary>
    /// Initializer which can handle regular C#
    /// </summary>
    public class RegularInitializer
    {
        private readonly string _code;
        private readonly RoslynPluginCatalogOptions _options;

        public RegularInitializer(string code, RoslynPluginCatalogOptions options)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentOutOfRangeException(nameof(code), code, "Script can not be null or empty");
            }

            _code = code;
            _options = options ?? new RoslynPluginCatalogOptions();
        }

        public Task<Assembly> CreateAssembly()
        {
            try
            {
                var generator = new AssemblyGenerator();
                generator.ReferenceAssemblyContainingType<Action>();

                if (_options.AdditionalReferences?.Any() == true)
                {
                    foreach (var assembly in _options.AdditionalReferences)
                    {
                        generator.ReferenceAssembly(assembly);
                    }
                }

                string assemblySourceCode;

                using (var sourceWriter = new SourceWriter())
                {
                    sourceWriter.UsingNamespace("System");
                    sourceWriter.UsingNamespace("System.Diagnostics");
                    sourceWriter.UsingNamespace("System.Threading.Tasks");
                    sourceWriter.UsingNamespace("System.Text");
                    sourceWriter.UsingNamespace("System.Collections");
                    sourceWriter.UsingNamespace("System.Collections.Generic");

                    if (_options.AdditionalNamespaces?.Any() == true)
                    {
                        foreach (var ns in _options.AdditionalNamespaces)
                        {
                            sourceWriter.UsingNamespace(ns);
                        }
                    }

                    sourceWriter.Write(_code);

                    assemblySourceCode = sourceWriter.Code();
                }

                var result = generator.Generate(assemblySourceCode);

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                throw new InvalidCodeException("Failed to create assembly from regular code. Code: " + _code, e);
            }
        }
    }
}
