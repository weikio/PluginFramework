using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Configuration.Converters;

namespace Weikio.PluginFramework.Catalogs.NuGet
{
    public class NugetPackageCatalogConfigurationConverter : IConfigurationToCatalogConverter
    {
        public bool CanConvert(string type)
        {
            return string.Equals(type, "NugetPackage", StringComparison.InvariantCultureIgnoreCase);
        }

        public IPluginCatalog Convert(IConfigurationSection configuration)
        {
            var packageName = configuration.GetValue<string>("Name")
                       ?? throw new ArgumentException("Plugin Framework's NugetPackageCatalog requires a NuGet package name.");

            var packageVersion = configuration.GetValue<string>("Version")
                              ?? throw new ArgumentException("Plugin Framework's NugetPackageCatalog requires a NuGet package version.");

            return new NugetPackagePluginCatalog(packageName, packageVersion, true);
        }
    }
}
