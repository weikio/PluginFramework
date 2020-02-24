using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Weikio.PluginFramework.Catalogs
{
    public class TypePluginCatalogOptions
    {
        public Func<TypePluginCatalogOptions, Type, string> PluginNameGenerator { get; set; } = (options, type) =>
        {
            var displayNameAttribute = type.GetCustomAttribute(typeof(DisplayNameAttribute), true) as DisplayNameAttribute;

            if (displayNameAttribute == null)
            {
                return type.FullName;
            }

            if (string.IsNullOrWhiteSpace(displayNameAttribute.DisplayName))
            {
                return type.FullName;
            }

            return displayNameAttribute.DisplayName;
        };
        
        public Func<TypePluginCatalogOptions, Type, Version> PluginVersionGenerator { get; set; } = (options, type) =>
        {
            var assemblyLocation = type.Assembly.Location;
            Version version;

            if (!string.IsNullOrWhiteSpace(assemblyLocation))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

                version = Version.Parse(versionInfo.FileVersion);
            }
            else
            {
                version = new Version(1, 0, 0, 0);
            }

            return version;
        };
        
        public Func<TypePluginCatalogOptions, Type, string> PluginDescriptionGenerator { get; set; } = (options, type) =>
        {
            var assemblyLocation = type.Assembly.Location;

            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                return string.Empty;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            return versionInfo.Comments;
        };
        
        public Func<TypePluginCatalogOptions, Type, string> PluginProductVersionGenerator { get; set; } = (options, type) =>
        {
            var assemblyLocation = type.Assembly.Location;

            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                return string.Empty;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            return versionInfo.ProductVersion;
        };
    }
}
