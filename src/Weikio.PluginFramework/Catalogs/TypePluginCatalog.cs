using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class TypePluginCatalog : IPluginCatalog
    {
        private readonly Type _pluginType;
        private PluginDefinition _pluginDefinition;

        public TypePluginCatalog(Type pluginType)
        {
            _pluginType = pluginType;
        }

        public Task Initialize()
        {
            var assemblyLocation = _pluginType.Assembly.Location;

            var version = GetVersion(assemblyLocation);
            var pluginName = GetPluginName();
            var modeDetails = GetMoreVersionDetails(assemblyLocation);

            _pluginDefinition = new PluginDefinition(pluginName, version, this, modeDetails.Description, modeDetails.ProductVersion);

            IsInitialized = true;

            return Task.CompletedTask;
        }

        private string GetPluginName()
        {
            if (string.IsNullOrWhiteSpace(_pluginType.Namespace))
            {
                return _pluginType.Name;
            }

            return _pluginType.Namespace.Split('.').Last();
        }

        private Version GetVersion(string assemblyLocation)
        {
            Version version;

            if (!string.IsNullOrWhiteSpace(assemblyLocation))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(_pluginType.Assembly.Location);

                version = Version.Parse(versionInfo.FileVersion);
            }
            else
            {
                version = new Version(1, 0, 0, 0);
            }

            return version;
        }

        private (string Description, string ProductVersion) GetMoreVersionDetails(string assemblyLocation)
        {
            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                return ("", "");
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(_pluginType.Assembly.Location);

            return (versionInfo.Comments, versionInfo.ProductVersion);
        }

        public bool IsInitialized { get; private set; }

        public Task<List<PluginDefinition>> GetAll()
        {
            var result = new List<PluginDefinition>() { _pluginDefinition };

            return Task.FromResult(result);
        }

        public Task<PluginDefinition> Get(string name, Version version)
        {
            if (!string.Equals(name, _pluginDefinition.Name, StringComparison.InvariantCultureIgnoreCase) ||
                version != _pluginDefinition.Version)
            {
                return Task.FromResult<PluginDefinition>(null);
            }

            return Task.FromResult(_pluginDefinition);
        }

        public Task<Assembly> GetAssembly(PluginDefinition definition)
        {
            return Task.FromResult(_pluginType.Assembly);
        }
    }
}
