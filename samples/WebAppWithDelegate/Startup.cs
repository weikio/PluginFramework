using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Delegates;

namespace WebAppWithDelegate
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Create a Plugin Catalog from an Action
            var actionDelegate = new Action<List<string>>(s =>
            {
                s.Add("Hello from action");
            });
            var actionCatalog = new DelegatePluginCatalog(actionDelegate, "MyActionPlugin");

            // 2. Create an another catalog from a Func
            var funcDelegate = new Func<List<string>, string>(s =>
            {
                s.Add("Hello from func");

                return string.Join(Environment.NewLine, s);
            });
            var funcCatalog = new DelegatePluginCatalog(funcDelegate, "MyFuncPlugin");

            // 3. Create a third catalog to show how constructor injection and parameters can be used
            var funcWithExternalService = new Func<string, ExternalService, string>((s, service) =>
            {
                var words = service.GetWords();

                s = s + Environment.NewLine + words;

                return s;
            });

            var funcWithExternalServiceCatalog = new DelegatePluginCatalog(funcWithExternalService, pluginName: "MyExternalServicePlugin",
                // 4. Use conversion rules to indicate that ExternalService should be a constructor parameter and the string should be a property
                conversionRules: new List<DelegateConversionRule>()
                {
                    // Rules can target parameter types
                    new DelegateConversionRule(info => info.ParameterType == typeof(ExternalService), nfo => new ParameterConversion() { ToConstructor = true }),
                    // Conversions based on the parameter names also work
                    new DelegateConversionRule(info => info.Name == "s", nfo => new ParameterConversion() { ToPublicProperty = true }),
                });

            services.AddPluginFramework()
                .AddPluginCatalog(new CompositePluginCatalog(actionCatalog, funcCatalog, funcWithExternalServiceCatalog));

            services.AddSingleton<ExternalService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
