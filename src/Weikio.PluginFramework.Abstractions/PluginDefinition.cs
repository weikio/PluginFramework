using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Abstractions
{
    public class PluginOld
    {
        public PluginOld(string name, Version version, IPluginCatalog source, string description = "", string productVersion = "")
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
        
        // public async Task<Plugin> GetPlugins()
        // {
        //     return await GetPlugins(new Dictionary<string, Predicate<Type>>());
        // }

        public async Task<List<Type>> GetTypes()
        {
            return await GetTypes(new Dictionary<string, Predicate<Type>>());
        }

        public async Task<List<Type>> GetTypes(Predicate<Type> filter)
        {
            var taggedFilters = new Dictionary<string, Predicate<Type>>() { { string.Empty, filter } };
            return await GetTypes(taggedFilters);
        }
        
        // public async Task<Plugin> GetPlugins(Predicate<Type> filter)
        // {
        //     var taggedFilters = new Dictionary<string, Predicate<Type>>() { { string.Empty, filter } };
        //
        //     return await GetPlugins(taggedFilters);
        // }

        // public async Task<Plugin> GetPlugins(Dictionary<string, Predicate<Type>> taggedFilters)
        // {
        //     var assembly = await Source.GetAssembly(this);
        //
        //     if (assembly == null)
        //     {
        //         throw new ArgumentException();
        //     }
        //
        //     var allTypes = assembly.GetExportedTypes();
        //     var taggedTypes = new List<(string Tag, Type Type)>();
        //
        //     if (taggedFilters?.Any() == true)
        //     {
        //         foreach (var taggedFilter in taggedFilters)
        //         {
        //             taggedTypes.AddRange(allTypes.Where(x => taggedFilter.Value(x)).Select(x => (taggedFilter.Key, x)));
        //         }
        //     }
        //     else
        //     {
        //         taggedTypes.AddRange(allTypes.Select(x => (string.Empty, x)));
        //     }
        //
        //     var result = new Plugin(this, assembly, taggedTypes);
        //
        //     return result;
        // }
        
        public async Task<List<Type>> GetTypes(Dictionary<string, Predicate<Type>> taggedFilters)
        {
            var assembly = await Source.GetAssembly(this);

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
            
            return taggedTypes.Select(x => x.Type).ToList();
        }
    }
}
