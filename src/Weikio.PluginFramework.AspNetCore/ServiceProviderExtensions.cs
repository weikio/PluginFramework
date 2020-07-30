using System;
using Weikio.PluginFramework.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static object Create(this IServiceProvider serviceProvider, Plugin plugin)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin);
        }
        
        public static T Create<T>(this IServiceProvider serviceProvider, Plugin plugin) where T : class
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin) as T;
        }
    }
}
