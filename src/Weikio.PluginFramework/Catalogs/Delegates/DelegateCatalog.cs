using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    public class DelegatePluginCatalog : IPluginCatalog
    {
        private AssemblyPluginCatalog _catalog;
        private readonly MulticastDelegate _multicastDelegate;

        private readonly DelegatePluginCatalogOptions _options;

        public DelegatePluginCatalog(MulticastDelegate multicastDelegate) : this(multicastDelegate, pluginName: null)
        {
        }
        /// <summary>
        /// Creates an instance of DelegatePluginCatalog 
        /// </summary>
        /// <param name="multicastDelegate">Plugin's delegate</param>
        /// <param name="pluginName">Name of the plugin</param>
        public DelegatePluginCatalog(MulticastDelegate multicastDelegate, string pluginName = "") : this(multicastDelegate, null, null, null, pluginName)
        {
        }

        public DelegatePluginCatalog(MulticastDelegate multicastDelegate, DelegatePluginCatalogOptions options) : this(multicastDelegate, 
            options?.ConversionRules, options?.NameOptions, options)
        {
        }

        public DelegatePluginCatalog(MulticastDelegate multicastDelegate,
            List<DelegateConversionRule> conversionRules = null,
            PluginNameOptions nameOptions = null, DelegatePluginCatalogOptions options = null, string pluginName = null)
        {
            if (multicastDelegate == null)
            {
                throw new ArgumentNullException(nameof(multicastDelegate));
            }

            _multicastDelegate = multicastDelegate;

            if (conversionRules == null)
            {
                conversionRules = new List<DelegateConversionRule>();
            }

            if (options != null)
            {
                _options = options;
            }
            else
            {
                _options = new DelegatePluginCatalogOptions();
            }

            _options.ConversionRules = conversionRules;

            if (nameOptions == null)
            {
                nameOptions = new PluginNameOptions();
            }

            _options.NameOptions = nameOptions;

            if (!string.IsNullOrWhiteSpace(pluginName))
            {
                _options.NameOptions.PluginNameGenerator = (pluginNameOptions, type) => pluginName;
            }
        }

        public async Task Initialize()
        {
            var converter = new DelegateToAssemblyConverter();
            var assembly = converter.CreateAssembly(_multicastDelegate, _options);

            var options = new AssemblyPluginCatalogOptions() { PluginNameOptions = _options.NameOptions };

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
