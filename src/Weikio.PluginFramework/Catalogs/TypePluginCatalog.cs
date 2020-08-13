using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    /// <summary>
    /// Plugin Catalog for a single .NET Type.
    /// </summary>
    public class TypePluginCatalog : IPluginCatalog
    {
        private readonly Type _pluginType;
        private readonly TypePluginCatalogOptions _options;
        private Plugin _plugin;

        public TypePluginCatalog(Type pluginType) : this(pluginType, null, null, null)
        {
        }

        public TypePluginCatalog(Type pluginType, PluginNameOptions nameOptions) : this(pluginType, null, nameOptions, null)
        {
        }

        public TypePluginCatalog(Type pluginType, Action<PluginNameOptions> configure) : this(pluginType, configure, null, null)
        {
        }

        public TypePluginCatalog(Type pluginType, TypePluginCatalogOptions options) : this(pluginType, null, null, options)
        {
        }

        public TypePluginCatalog(Type pluginType, Action<PluginNameOptions> configure, PluginNameOptions nameOptions, TypePluginCatalogOptions options)
        {
            if (pluginType == null)
            {
                throw new ArgumentNullException(nameof(pluginType));
            }

            _pluginType = pluginType;
            _options = options ?? new TypePluginCatalogOptions();

            if (_options.TypeFinderCriterias == null)
            {
                _options.TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
            }

            if (_options.TypeFinderCriterias.Any() != true)
            {
                _options.TypeFinderCriterias.Add(string.Empty, new TypeFinderCriteria()
                {
                    Query = (context, type) => true
                });
            }

            if (_options.TypeFindingContext == null)
            {
                _options.TypeFindingContext = new PluginAssemblyLoadContext(pluginType.Assembly);
            }
            
            if (configure == null && nameOptions == null)
            {
                return;
            }

            var naming = nameOptions ?? new PluginNameOptions();
            configure?.Invoke(naming);

            _options.PluginNameOptions = naming;
        }

        public Task Initialize()
        {
            var namingOptions = _options.PluginNameOptions;
            var version = namingOptions.PluginVersionGenerator(namingOptions, _pluginType);
            var pluginName = namingOptions.PluginNameGenerator(namingOptions, _pluginType);
            var description = namingOptions.PluginDescriptionGenerator(namingOptions, _pluginType);
            var productVersion = namingOptions.PluginProductVersionGenerator(namingOptions, _pluginType);

            var tag = string.Empty;

            var finder = new TypeFinder();

            foreach (var typeFinderCriteria in _options.TypeFinderCriterias)
            {
                var isMatch = finder.IsMatch(typeFinderCriteria.Value, _pluginType, _options.TypeFindingContext);

                if (isMatch)
                {
                    tag = typeFinderCriteria.Key;

                    break;
                }
            }

            _plugin = new Plugin(_pluginType.Assembly, _pluginType, pluginName, version, this, description, productVersion, tag);

            IsInitialized = true;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public bool IsInitialized { get; private set; }

        /// <inheritdoc />
        public List<Plugin> GetPlugins()
        {
            return new List<Plugin>() { _plugin };
        }

        /// <inheritdoc />
        public Plugin Get(string name, Version version)
        {
            if (!string.Equals(name, _plugin.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _plugin.Version)
            {
                return null;
            }

            return _plugin;
        }
    }
}
