using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.AspNetCore;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Samples.SharedPlugins;

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
            var folderPluginCatalog = new FolderPluginCatalog(@"..\Shared\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1", type =>
            {
                type.Implements<IOperator>();
            });

            services.AddPluginFramework()
                .AddPluginCatalog(folderPluginCatalog)
                .AddPluginType<IOperator>();

            // Alternatively
            // services.AddPluginFramework<IOperator>(@"..\Shared\Weikio.PluginFramework.Samples.SharedPlugins\bin\debug\netcoreapp3.1");

            // Default plugin type returned can be optionally configured with DefaultType function
            //services.AddPluginFramework()
            //    .AddPluginCatalog(folderPluginCatalog)
            //    .AddPluginType<IOperator>(configureDefault: option =>
            //    {
            //        option.DefaultType = (serviceProvider, implementingTypes) => typeof(MultiplyOperator);
            //    });

            // Alternatively default plugin type can be also configured with Configure and provided with the same DefaultType function
            //services.Configure<DefaultPluginOption>(nameof(IOperator), option =>
            //    option.DefaultType = (serviceProvider, implementingTypes) => typeof(MultiplyOperator));

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
