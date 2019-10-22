using System;

namespace Weikio.PluginFramework
{
    public class PluginDefinition
    {
        public PluginDefinition(string name, Version version, IPluginCatalog source)
        {
            Name = name;
            Version = version;
            Source = source;
        }

        public string Name { get; }
        public Version Version { get; }
        public IPluginCatalog Source { get; }

        public override string ToString()
        {
            return $"{Name}: {Version}";
        }
    }
}
