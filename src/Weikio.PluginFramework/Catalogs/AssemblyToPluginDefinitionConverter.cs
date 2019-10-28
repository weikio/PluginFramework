using System;
using System.Diagnostics;
using System.Reflection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyToPluginDefinitionConverter
    {
        public static PluginDefinition Convert(Assembly assembly, IPluginCatalog source)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            var result = new PluginDefinition(assembly.GetName().Name, Version.Parse(fileVersion.FileVersion), source);

            return result;
        }
    }
}
