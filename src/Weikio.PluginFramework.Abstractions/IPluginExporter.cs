using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public interface IPluginExporter
    {
        Task<Plugin> Get(PluginDefinition definition);
        Task<Plugin> Get(PluginDefinition definition, Predicate<Type> filter);
        Task<Plugin> Get(PluginDefinition definition, Dictionary<string, Predicate<Type>> taggedFilters);
    }
}
