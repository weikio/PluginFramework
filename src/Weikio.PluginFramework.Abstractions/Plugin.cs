using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Weikio.PluginFramework.Abstractions
{
    public class Plugin
    {
        public PluginDefinition Definition { get; }
        public Assembly Assembly { get; }
        public List<(string Tag, Type Type)> PluginTypes { get; }

        public List<Type> Types
        {
            get
            {
                return PluginTypes.Select(x => x.Type).ToList();
            }
        }

        public Plugin(PluginDefinition definition, Assembly assembly, List<(string Tag, Type Type)> pluginTypes)
        {
            Definition = definition;
            Assembly = assembly;
            PluginTypes = pluginTypes;
        }
    }
}
