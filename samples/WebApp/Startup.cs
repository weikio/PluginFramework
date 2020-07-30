using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace WebApp
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
            var folderPluginCatalog = new FolderPluginCatalog(@"..\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1", type =>
            {
                type.Implements<IOperator>();
            });

            services.AddPluginFramework()
                .AddPluginCatalog(folderPluginCatalog)
                .AddPluginType<IOperator>();

            // Alternatively
            // services.AddPluginFramework<IOperator>(@"..\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1");

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
