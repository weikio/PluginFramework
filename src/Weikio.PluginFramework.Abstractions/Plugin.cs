using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weikio.PluginFramework.Abstractions
{
    public class Plugin
    {
        public Assembly Assembly { get; }
        public Type Type { get; }
        public string Name { get; }
        public Version Version { get; }
        public IPluginCatalog Source { get; }
        public string Description { get; }
        public string ProductVersion { get; }
        public string Tag { get; }

        public Plugin(Assembly assembly, Type type, string name, Version version, IPluginCatalog source, string description = "", string productVersion = "", string tag = "")
        {
            Assembly = assembly;
            Type = type;
            Name = name;
            Version = version;
            Source = source;
            Description = description;
            ProductVersion = productVersion;
            Tag = tag;
        }
        
        public static implicit operator Type(Plugin plugin)
        {
            return plugin.Type;
        }
        
        public override string ToString()
        {
            return $"{Name}: {Version}";
        }
    }
}
