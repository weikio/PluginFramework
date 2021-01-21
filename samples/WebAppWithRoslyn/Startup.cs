using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.PluginFramework.AspNetCore;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.Roslyn;

namespace WebAppWithRoslyn
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // First we create a Roslyn Plugin Catalog which uses the scripting version of C#/Roslyn
            var script = "var x = \"Hello from Roslyn Plugin\"; return x;";
            var roslynScriptCatalog = new RoslynPluginCatalog(script);

            // We also create an another Roslyn Plugin Catalog to show how to create a plugin from a class. 
            // This catalog also uses dependency injection, external references and additional namespaces.
            var code = @"public class MyClass
                   {
                       private ExternalService _service;
                       public MyClass(ExternalService service)
                       {
                            _service = service;
                       } 

                       public string RunThings()
                       {
                            var result = JsonConvert.SerializeObject(15);
                            result += _service.DoWork();
                            return result; 
                       }
                   }";

            var options = new RoslynPluginCatalogOptions()
            {
                PluginName = "MyPlugin",
                PluginVersion = new Version("1.5.0.0"),
                AdditionalReferences = new List<Assembly>() { typeof(Newtonsoft.Json.JsonConvert).Assembly, typeof(ExternalService).Assembly },
                AdditionalNamespaces = new List<string>() { "Newtonsoft.Json", "WebAppWithRoslyn" }
            };
            
            var roslynCodeCatalog = new RoslynPluginCatalog(code, options);

            services.AddPluginFramework()
                .AddPluginCatalog(new CompositePluginCatalog(roslynScriptCatalog, roslynCodeCatalog));

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
