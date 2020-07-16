using System.Collections.Generic;
using Weikio.PluginFramework.Context;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyPluginCatalogOptions
    {
        public PluginLoadContextOptions PluginLoadContextOptions = new PluginLoadContextOptions();
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
    }
}
