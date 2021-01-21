using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions.DependencyInjection;
using Weikio.PluginFramework.Configuration.Converters;

namespace Weikio.PluginFramework.Catalogs.NuGet
{
    public static class PluginFrameworkBuilderExtensions
    {
        public static IPluginFrameworkBuilder AddNugetConfiguration(this IPluginFrameworkBuilder builder)
        {
            builder.Services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(NugetFeedCatalogConfigurationConverter));
            builder.Services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(NugetPackageCatalogConfigurationConverter));

            return builder;
        }
    }
}
