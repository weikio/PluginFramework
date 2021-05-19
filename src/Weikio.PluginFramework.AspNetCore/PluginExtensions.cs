using System;
using Weikio.PluginFramework.Abstractions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class PluginExtensions
    {
        public static object Create(this Plugin plugin, IServiceProvider serviceProvider, params object[] parameters)
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin, parameters);
        }
        
        public static T Create<T>(this Plugin plugin, IServiceProvider serviceProvider, params object[] parameters) where T : class
        {
            return ActivatorUtilities.CreateInstance(serviceProvider, plugin, parameters) as T;
        }
    }
}
