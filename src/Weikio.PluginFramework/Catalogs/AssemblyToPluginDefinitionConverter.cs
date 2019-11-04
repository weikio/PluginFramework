using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyToPluginDefinitionConverter
    {
        public static PluginDefinition Convert(Assembly assembly, IPluginCatalog source)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var result = new PluginDefinition(assembly.GetName().Name, Version.Parse(versionInfo.FileVersion), source, versionInfo.Comments, versionInfo.ProductVersion);

            return result;
        }
    }
}
