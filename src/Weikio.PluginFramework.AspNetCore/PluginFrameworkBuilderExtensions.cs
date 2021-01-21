using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Abstractions.DependencyInjection;

namespace Weikio.PluginFramework.AspNetCore
{
    public static class PluginFrameworkBuilderExtensions
    {
        public static IPluginFrameworkBuilder AddPluginCatalog(this IPluginFrameworkBuilder builder, IPluginCatalog pluginCatalog)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPluginCatalog), pluginCatalog));

            return builder;
        }

        public static IPluginFrameworkBuilder AddPluginType<T>(this IPluginFrameworkBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where T : class
        {
            var serviceDescriptorEnumerable = new ServiceDescriptor(typeof(IEnumerable<T>), sp =>
            {
                var pluginProvider = sp.GetService<PluginProvider>();
                var result = pluginProvider.GetTypes<T>();

                return result.AsEnumerable();
            }, serviceLifetime);

            var serviceDescriptorSingle = new ServiceDescriptor(typeof(T), sp =>
            {
                var pluginProvider = sp.GetService<PluginProvider>();
                var result = pluginProvider.GetTypes<T>();

                return result.FirstOrDefault();
            }, serviceLifetime);

            builder.Services.Add(serviceDescriptorEnumerable);
            builder.Services.Add(serviceDescriptorSingle);

            return builder;
        }
    }
}
