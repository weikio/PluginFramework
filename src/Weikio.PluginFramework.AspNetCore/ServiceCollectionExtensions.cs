using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.AspNetCore;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Configuration;
using Weikio.PluginFramework.Configuration.Converters;
using Weikio.PluginFramework.Configuration.Providers;
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

        /// <summary>
        /// Add plugins from the provided <see cref="IConfiguration"/> object.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> on which the plugins will be added.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> object that contains the plugin configurations.</param>
        /// <returns>This <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddPluginFramework(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register the default implementation of IPluginCatalogConfigurationLoader with the provided configuration.
            services.AddTransient<IPluginCatalogConfigurationLoader>(serviceProvider =>
                new PluginCatalogConfigurationLoader(configuration));

            services.TryAddSingleton((Func<IServiceProvider, IPluginCatalog>)(serviceProvider =>
            {
                // Grab all the IPluginCatalogConfigurationLoader implementations to laod catalog configurations.
                var loaders = serviceProvider
                    .GetServices<IPluginCatalogConfigurationLoader>()
                    .ToList();

                var converters = serviceProvider.GetServices<IConfigurationToCatalogConverter>();
                var catalogs = new List<IPluginCatalog>();

                foreach (var loader in loaders)
                {
                    // Load the catalog configurations.
                    var catalogConfigs = loader.GetCatalogConfigurations();

                    if (catalogConfigs == null || catalogConfigs.Count == 0)
                    {
                        // if no configurations were provided continue.
                        continue;
                    }

                    for (var i = 0; i < catalogConfigs.Count; i++)
                    {
                        var item = catalogConfigs[i];
                        var key = $"{loader.SectionKey}:{loader.CatalogsKey}:{i}";

                        // Check if a type is provided.
                        var type = string.IsNullOrWhiteSpace(item.Type)
                               ? throw new ArgumentException($"A type must be provided for catalog at position {i + 1}")
                               : item.Type;

                        // Try to find any registered converter that can convert the specified type.
                        var foundConverter = converters.FirstOrDefault(converter => converter.CanConvert(type));

                        // Add a catalog to the list of catalogs.
                        catalogs.Add(foundConverter != null
                            // If a converter was found we call it's convert method.
                            ? foundConverter.Convert(loader.Configuration.GetSection(key))
                            // If no converter was found (it's null) we proceed with the built-in type converters.
                            : type switch
                            {
                                // Assembly type.
                                CatalogTypes.Assembly => new AssemblyCatalogConfigurationCoverter().Convert(configuration.GetSection(key)),

                                // Folder type.
                                CatalogTypes.Folder => new FolderCatalogConfigurationConverter().Convert(configuration.GetSection(key)),

                                // Unkown type.
                                _ => throw new ArgumentException($"The type provided for catalog at position {i + 1} is unknown.")
                            });
                    }
                }

                return new CompositePluginCatalog(catalogs.ToArray());
            }));

            services.AddPluginFramework();
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
