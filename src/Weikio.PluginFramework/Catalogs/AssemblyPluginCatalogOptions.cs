using System.Collections.Generic;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalogOptions
    {
        public PluginLoadContextOptions PluginLoadContextOptions = new PluginLoadContextOptions();
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
    }
}
