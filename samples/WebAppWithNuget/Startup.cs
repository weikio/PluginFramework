using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.Samples.Shared;

namespace WebAppWithNuget
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
            NugetPluginCatalogOptions.Defaults.LoggerFactory = () => new NugetLogger(services);

            var options = new NugetPluginCatalogOptions
            {
                ForcePackageCaching = true, 
                CustomPackagesFolder = Path.Combine(Path.GetTempPath(), "NugetPackagePluginCatalog", "Sample")
            };

            var nugetCatalog = new NugetPackagePluginCatalog("Weikio.PluginFramework.Samples.SharedPlugins", includePrerelease: true, configureFinder: finder =>
            {
                finder.Implements<IOperator>();
            }, options: options);

            services.AddPluginFramework()
                .AddPluginCatalog(nugetCatalog)
                .AddPluginType<IOperator>();

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
