using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Weikio.PluginFramework.Abstractions
{
    /// <summary>
    /// Configuration options for defining how the plugin name and plugin version are generated.
    /// </summary>
    public class PluginNameOptions
    {
        /// <summary>
        /// Gets or sets the func which defines how the plugin's name is deducted. By default tries to find the plugin's name from <see cref="DisplayNameAttribute"/>.
        /// If that is missing, type's full name is used. 
        /// </summary>
        public Func<PluginNameOptions, Type, string> PluginNameGenerator { get; set; } = (options, type) =>
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

        /// <summary>
        /// Gets or sets the func which defines how the plugin's version is deducted. By default tries to find the plugin's assembly and its FileVersion.
        /// </summary>
        public Func<PluginNameOptions, Type, Version> PluginVersionGenerator { get; set; } = (options, type) =>
        {
            var assemblyLocation = type.Assembly.Location;
            Version version;

            if (!string.IsNullOrWhiteSpace(assemblyLocation))
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

                if (string.IsNullOrWhiteSpace(versionInfo.FileVersion))
                {
                    version = new Version(1, 0, 0, 0);
                }
                else if (string.Equals(versionInfo.FileVersion, "0.0.0.0"))
                {
                    version = new Version(1, 0, 0, 0);
                }
                else
                {
                    version = Version.Parse(versionInfo.FileVersion);
                }
            }
            else
            {
                version = new Version(1, 0, 0, 0);
            }

            return version;
        };

        /// <summary>
        /// Gets or sets the func which defines how the plugin's description is deducted. By default tries to find the plugin's assembly and its Comments.
        /// </summary>
        public Func<PluginNameOptions, Type, string> PluginDescriptionGenerator { get; set; } = (options, type) =>
        {
            var assemblyLocation = type.Assembly.Location;

            if (string.IsNullOrWhiteSpace(assemblyLocation))
            {
                return string.Empty;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(assemblyLocation);

            return versionInfo.Comments;
        };

        /// <summary>
        /// Gets or sets the func which defines how the plugin's product version is deducted. By default tries to find the plugin's assembly and its ProductVersion.
        /// </summary>
        public Func<PluginNameOptions, Type, string> PluginProductVersionGenerator { get; set; } = (options, type) =>
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
