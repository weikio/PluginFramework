using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs
{
    public class AssemblyToPluginDefinitionConverter
    {
        public static PluginOld Convert(Assembly assembly, IPluginCatalog source)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            var result = new PluginOld(assembly.GetName().Name, Version.Parse(versionInfo.FileVersion), source, versionInfo.Comments, versionInfo.ProductVersion);

            return result;
        }
    }
}
