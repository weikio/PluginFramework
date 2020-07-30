using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Weikio.PluginFramework.Catalogs
{
    public static class DefaultPluginResolver
    {
        public static bool Resolve(MetadataReader metadataReader, TypeDefinition typeDefinition, HashSet<string> pluginAssemblyNames)
        {
            if (pluginAssemblyNames == null || !metadataReader.IsAssembly)
            {
                return false;
            }

            var assemblyName = metadataReader.GetString(metadataReader.GetAssemblyDefinition().Name);

            if (pluginAssemblyNames.Contains(assemblyName))
            {
                return true;
            }

            return false;
        }
    }
}
