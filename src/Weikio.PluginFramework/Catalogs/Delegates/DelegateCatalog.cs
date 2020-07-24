using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    public class DelegatePluginCatalog : IPluginCatalog
    {
        private AssemblyPluginCatalog _catalog;
        private readonly MulticastDelegate _multicastDelegate;
        private readonly List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)> _conversionRules;
        private readonly PluginNameOptions _nameOptions;

        public DelegatePluginCatalog(MulticastDelegate multicastDelegate, List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)> conversionRules = null, 
            PluginNameOptions nameOptions = null)
        {
            if (multicastDelegate == null)
            {
                throw new ArgumentNullException(nameof(multicastDelegate));
            }

            _multicastDelegate = multicastDelegate;

            if (conversionRules == null)
            {
                conversionRules = new List<(Predicate<ParameterInfo>, Func<ParameterInfo, ParameterConversion>)>();
            }

            _conversionRules = conversionRules;

            if (nameOptions == null)
            {
                nameOptions = new PluginNameOptions();
            }
            
            _nameOptions = nameOptions;
        }

        public async Task Initialize()
        {
            var converter = new DelegateToAssemblyConverter();
            var assembly = converter.CreateAssembly(_multicastDelegate, _conversionRules);

            var options = new AssemblyPluginCatalogOptions()
            {
                PluginNameOptions = _nameOptions
            };
            
            _catalog = new AssemblyPluginCatalog(assembly, options);
            await _catalog.Initialize();
            
            IsInitialized = true;
        }

        public bool IsInitialized { get; set; }
        public List<Plugin> GetPlugins()
        {
            return _catalog.GetPlugins();
        }

        public Plugin Get(string name, Version version)
        {
            return _catalog.Get(name, version);
        }
    }
}
