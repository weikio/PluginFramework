using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.TypeFinding;
using Weikio.TypeGenerator;
using Weikio.TypeGenerator.Delegates;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    public class DelegatePluginCatalog : IPluginCatalog
    {
        private TypePluginCatalog _catalog;
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
            var converter = new DelegateToTypeWrapper();

            // Convert this catalog's options to the format supported by Delegate Wrapper.
            // TODO: At some good point change the catalog so that it uses the Delegate Wrapper's options instead of defining its own.
            var delegateToTypeWrapperOptions = ConvertOptions();
            var assembly = converter.CreateType(_multicastDelegate, delegateToTypeWrapperOptions);

            var options = new TypePluginCatalogOptions() { PluginNameOptions = _options.NameOptions };

            if (_options.Tags?.Any() == true)
            {
                options.TypeFinderOptions = new TypeFinderOptions
                {
                    TypeFinderCriterias = new List<TypeFinderCriteria> { TypeFinderCriteriaBuilder.Create().Tag(_options.Tags.ToArray()) }
                };
            }

            _catalog = new TypePluginCatalog(assembly, options);
            await _catalog.Initialize();

            IsInitialized = true;
        }

        private DelegateToTypeWrapperOptions ConvertOptions()
        {
            var convRules = GetConversionRules();

            return new DelegateToTypeWrapperOptions()
            {
                ConversionRules = convRules,
                MethodName = _options.MethodName,
                NamespaceName = _options.NamespaceName,
                TypeName = _options.TypeName,
                MethodNameGenerator = wrapperOptions => _options.MethodNameGenerator(_options),
                NamespaceNameGenerator = wrapperOptions => _options.NamespaceNameGenerator(_options),
                TypeNameGenerator = wrapperOptions => _options.TypeNameGenerator(_options),
            };
        }

        private List<ParameterConversionRule> GetConversionRules()
        {
            var convRules = new List<ParameterConversionRule>();

            foreach (var conversionRule in _options.ConversionRules)
            {
                var paramConversion = new ParameterConversionRule(conversionRule.CanHandle, info =>
                {
                    var handleResult = conversionRule.Handle(info);

                    return new TypeGenerator.ParameterConversion()
                    {
                        Name = handleResult.Name, ToConstructor = handleResult.ToConstructor, ToPublicProperty = handleResult.ToPublicProperty
                    };
                });

                convRules.Add(paramConversion);
            }

            return convRules;
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
