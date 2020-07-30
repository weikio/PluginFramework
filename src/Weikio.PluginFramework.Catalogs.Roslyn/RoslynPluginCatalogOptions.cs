using System;
using System.Collections.Generic;
using System.Reflection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs.Roslyn
{
    public class RoslynPluginCatalogOptions
    {
        private string _pluginName = "RoslynCode";
        private Version _pluginVersion = new Version(1, 0, 0);

        public string PluginName
        {
            get => _pluginName;
            set
            {
                _pluginName = value;

                PluginNameOptions.PluginNameGenerator = (options, type) => _pluginName;
            }
        }

        public Version PluginVersion
        {
            get => _pluginVersion;
            set
            {
                _pluginVersion = value;

                PluginNameOptions.PluginVersionGenerator = (options, type) => _pluginVersion;
            }
        }

        public string TypeName { get; set; } = "GeneratedType";
        public string NamespaceName { get; set; } = "GeneratedNamespace";
        public string MethodName { get; set; } = "Run";
        public bool ReturnsTask { get; set; } = true;
        public Func<RoslynPluginCatalogOptions, string> TypeNameGenerator { get; set; } = options => options.TypeName;
        public Func<RoslynPluginCatalogOptions, string> NamespaceNameGenerator { get; set; } = options => options.NamespaceName;
        public Func<RoslynPluginCatalogOptions, string> MethodNameGenerator { get; set; } = options => options.MethodName;
        public List<Assembly> AdditionalReferences { get; set; } = new List<Assembly>();
        public List<string> AdditionalNamespaces { get; set; } = new List<string>();
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
    }
}
