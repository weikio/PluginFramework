using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.PluginFramework
{
    public class PluginExporter : IPluginExporter
    {
        public async Task<Plugin> Get(PluginDefinition definition)
        {
            return await Get(definition, new Dictionary<string, Predicate<Type>>());
        }

        public async Task<Plugin> Get(PluginDefinition definition, Predicate<Type> filter)
        {
            var taggedFilters = new Dictionary<string, Predicate<Type>>() {{string.Empty, filter}};

            return await Get(definition, taggedFilters);
        }

        public async Task<Plugin> Get(PluginDefinition definition, Dictionary<string, Predicate<Type>> taggedFilters)
        {
            var assembly = await definition.Source.GetAssembly(definition);

            if (assembly == null)
            {
                throw new ArgumentException();
            }

            var allTypes = assembly.GetExportedTypes();
            var taggedTypes = new List<(string Tag, Type Type)>();

            if (taggedFilters?.Any() == true)
            {
                foreach (var taggedFilter in taggedFilters)
                {
                    taggedTypes.AddRange(allTypes.Where(x => taggedFilter.Value(x)).Select(x => (taggedFilter.Key, x)));
                }
            }
            else
            {
                taggedTypes.AddRange(allTypes.Select(x => (string.Empty, x)));
            }

            var result = new Plugin(definition, assembly, taggedTypes);

            return result;
        }
    }
}
