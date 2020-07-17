using System;

namespace Weikio.PluginFramework.Abstractions
{
    public class PluginDefinition
    {
        public PluginDefinition(string name, Version version, IPluginCatalog source, string description = "", string productVersion = "")
        {
            Name = name;
            Version = version;
            Source = source;
            Description = description;
            ProductVersion = productVersion;
        }

        public string Name { get; }
        public Version Version { get; }
        public IPluginCatalog Source { get; }
        public string Description { get; }
        public string ProductVersion { get; }

        public override string ToString()
        {
            return $"{Name}: {Version}";
        }
    }
}
