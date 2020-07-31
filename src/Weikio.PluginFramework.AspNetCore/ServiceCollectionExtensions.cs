using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.AspNetCore;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPluginFramework(this IServiceCollection services)
        {
            services.AddHostedService<PluginFrameworkInitializer>();

            services.AddSingleton(sp =>
            {
                var result = new List<Plugin>();
                var catalogs = sp.GetServices<IPluginCatalog>();

                foreach (var catalog in catalogs)
                {
                    var plugins = catalog.GetPlugins();

                    result.AddRange(plugins);
                }

                return result.AsEnumerable();
            });

            var aspNetCoreControllerAssemblyLocation = typeof(Controller).Assembly.Location;

            if (string.IsNullOrWhiteSpace(aspNetCoreControllerAssemblyLocation))
            {
                return services;
            }

            var aspNetCoreLocation = Path.GetDirectoryName(aspNetCoreControllerAssemblyLocation);
            
            if (PluginLoadContextOptions.Defaults.AdditionalRuntimePaths == null)
            {
                PluginLoadContextOptions.Defaults.AdditionalRuntimePaths = new List<string>();
            }

            if (!PluginLoadContextOptions.Defaults.AdditionalRuntimePaths.Contains(aspNetCoreLocation))
            {
                PluginLoadContextOptions.Defaults.AdditionalRuntimePaths.Add(aspNetCoreLocation);
            }

            return services;
        }
        
        public static IServiceCollection AddPluginFramework<TType>(this IServiceCollection services, string dllPath = "") where TType : class
        {
            services.AddPluginFramework();

            if (string.IsNullOrWhiteSpace(dllPath))
            {
                var entryAssembly = Assembly.GetEntryAssembly();

                if (entryAssembly == null)
                {
                    dllPath = Environment.CurrentDirectory;
                }
                else
                {
                    dllPath = Path.GetDirectoryName(entryAssembly.Location);
                }
            }

            var typeFinderCriteria = TypeFinderCriteriaBuilder.Create()
                .AssignableTo(typeof(TType))
                .Build();

            var catalog = new FolderPluginCatalog(dllPath, typeFinderCriteria);
            services.AddPluginCatalog(catalog);

            services.AddPluginType<TType>();
            
            return services;
        }

        public static IServiceCollection AddPluginCatalog(this IServiceCollection services, IPluginCatalog pluginCatalog)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPluginCatalog), pluginCatalog));

            return services;
        }

        public static IServiceCollection AddPluginType<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where T : class
        {
            var serviceDescriptorEnumerable = new ServiceDescriptor(typeof(IEnumerable<T>), sp =>
            {
                var result = GetTypes<T>(sp);

                return result.AsEnumerable();
                
            }, serviceLifetime);
            
            var serviceDescriptorSingle = new ServiceDescriptor(typeof(T), sp =>
            {
                var result = GetTypes<T>(sp);

                return result.FirstOrDefault();
                
            }, serviceLifetime);

            services.Add(serviceDescriptorEnumerable);
            services.Add(serviceDescriptorSingle);

            return services;
        }

        private static List<T> GetTypes<T>(IServiceProvider sp) where T : class
        {
            var result = new List<T>();
            var catalogs = sp.GetServices<IPluginCatalog>();

            foreach (var catalog in catalogs)
            {
                var plugins = catalog.GetPlugins();

                foreach (var plugin in plugins.Where(x => typeof(T).IsAssignableFrom(x)))
                {
                    var op = plugin.Create<T>(sp);

                    result.Add(op);
                }
            }

            return result;
        }
    }
}
