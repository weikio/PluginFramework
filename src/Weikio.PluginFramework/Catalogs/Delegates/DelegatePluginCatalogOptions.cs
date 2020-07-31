using System;
using System.Collections.Generic;
using System.Reflection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    public class DelegatePluginCatalogOptions
    {
        public PluginNameOptions NameOptions { get; set; } = new PluginNameOptions();
        public List<DelegateConversionRule> ConversionRules { get; set; } = new List<DelegateConversionRule>();
        public string MethodName { get; set; } = "Run";
        public string TypeName { get; set; } = "GeneratedType";
        public string NamespaceName { get; set; } = "GeneratedNamespace";
        public Func<DelegatePluginCatalogOptions, string> MethodNameGenerator { get; set; } = options => options.MethodName;
        public Func<DelegatePluginCatalogOptions, string> TypeNameGenerator { get; set; } = options => options.TypeName;
        public Func<DelegatePluginCatalogOptions, string> NamespaceNameGenerator { get; set; } = options => options.NamespaceName;
    }
}
