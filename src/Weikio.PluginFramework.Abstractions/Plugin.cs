using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weikio.PluginFramework.Abstractions
{
    /// <summary>
    /// Represents a single Plugin. Each plugin has a name, version and .NET Type. 
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the plugin version
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the .NET Type which is the plugin
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the plugin type's assembly
        /// </summary>
        public Assembly Assembly { get; }

        /// <summary>
        /// Gets the catalog which contains the plugins
        /// </summary>
        public IPluginCatalog Source { get; }

        /// <summary>
        /// Gets the description of the plugin
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the description of the plugin
        /// </summary>
        public string ProductVersion { get; }
        
        /// <summary>
        /// Gets the tag of the plugin
        /// </summary>
        public string Tag
        {
            get
            {
                return Tags.FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the tags of the plugin
        /// </summary>
        public List<string> Tags { get; }
        
        public Plugin(Assembly assembly, Type type, string name, Version version, IPluginCatalog source, string description = "", string productVersion = "",
            string tag = "", List<string> tags = null)
        {
            Assembly = assembly;
            Type = type;
            Name = name;
            Version = version;
            Source = source;
            Description = description;
            ProductVersion = productVersion;
            Tags = tags;

            if (Tags == null)
            {
                Tags = new List<string>();
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                Tags.Add(tag);
            }
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
