using System;
using Weikio.PluginFramework.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class PluginExtensions
    {
        public static object Create(this Plugin plugin, IServiceProvider serviceProvider)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin);
        }
        
        public static T Create<T>(this Plugin plugin, IServiceProvider serviceProvider) where T : class
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin) as T;
        }
    }
}
