using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Weikio.TypeGenerator;

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
                var generator = new CodeToAssemblyGenerator();
                generator.ReferenceAssemblyContainingType<Action>();

                if (_options.AdditionalReferences?.Any() == true)
                {
                    foreach (var assembly in _options.AdditionalReferences)
                    {
                        generator.ReferenceAssembly(assembly);
                    }
                }

                var code = new StringBuilder();
                code.AppendLine("using System;");
                code.AppendLine("using System.Diagnostics;");
                code.AppendLine("using System.Threading.Tasks;");
                code.AppendLine("using System.Text;");
                code.AppendLine("using System.Collections;");
                code.AppendLine("using System.Collections.Generic;");
                
                if (_options.AdditionalNamespaces?.Any() == true)
                {
                    foreach (var ns in _options.AdditionalNamespaces)
                    {
                        code.AppendLine($"using {ns};");
                    }
                }

                code.AppendLine(_code);
                var assemblySourceCode = code.ToString();

                var result = generator.GenerateAssembly(assemblySourceCode);

                return Task.FromResult(result);
            }
            catch (Exception e)
            {
                throw new InvalidCodeException("Failed to create assembly from regular code. Code: " + _code, e);
            }
        }
    }
}
