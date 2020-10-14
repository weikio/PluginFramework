using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
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
        public static IServiceCollection AddPluginFramework(this IServiceCollection services, Action<PluginFrameworkOptions> configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            
            services.AddHostedService<PluginFrameworkInitializer>();
            services.AddTransient<PluginProvider>();

            services.TryAddTransient(typeof(IPluginCatalogConfigurationLoader), typeof(PluginCatalogConfigurationLoader));
            services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(FolderCatalogConfigurationConverter));
            services.AddTransient(typeof(IConfigurationToCatalogConverter), typeof(AssemblyCatalogConfigurationCoverter));

            services.AddConfiguration();

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
        /// Add plugins from the IConfiguration.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> on which the plugins will be added.</param>
        /// <returns>This <see cref="IServiceCollection"/>.</returns>
        private static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            services.TryAddSingleton<IPluginCatalog>(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptions<PluginFrameworkOptions>>().Value;

                if (options.UseConfiguration == false)
                {
                    return new EmptyPluginCatalog();
                }
                
                // Grab all the IPluginCatalogConfigurationLoader implementations to load catalog configurations.
                var loaders = serviceProvider
                    .GetServices<IPluginCatalogConfigurationLoader>()
                    .ToList();

                var configuration = serviceProvider.GetService<IConfiguration>();

                var converters = serviceProvider.GetServices<IConfigurationToCatalogConverter>().ToList();
                var catalogs = new List<IPluginCatalog>();

                foreach (var loader in loaders)
                {
                    // Load the catalog configurations.
                    var catalogConfigs = loader.GetCatalogConfigurations(configuration);

                    if (catalogConfigs?.Any() != true)
                    {
                        continue;
                    }

                    for (var i = 0; i < catalogConfigs.Count; i++)
                    {
                        var item = catalogConfigs[i];
                        var key = $"{options.ConfigurationSection}:{loader.CatalogsKey}:{i}";

                        // Check if a type is provided.
                        if (string.IsNullOrWhiteSpace(item.Type))
                        {
                            throw new ArgumentException($"A type must be provided for catalog at position {i + 1}");
                        }

                        // Try to find any registered converter that can convert the specified type.
                        var foundConverter = converters.FirstOrDefault(converter => converter.CanConvert(item.Type));

                        if (foundConverter == null)
                        {
                            throw new ArgumentException($"The type provided for Plugin catalog at position {i + 1} is unknown.");
                        }

                        var catalog = foundConverter.Convert(configuration.GetSection(key));

                        catalogs.Add(catalog);
                    }
                }

                return new CompositePluginCatalog(catalogs.ToArray());
            });

            return services;
        }

        public static IServiceCollection AddPluginCatalog(this IServiceCollection services, IPluginCatalog pluginCatalog)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPluginCatalog), pluginCatalog));

            return services;
        }

        public static IServiceCollection AddPluginType<T>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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

            services.Add(serviceDescriptorEnumerable);
            services.Add(serviceDescriptorSingle);

            return services;
        }
    }
}
