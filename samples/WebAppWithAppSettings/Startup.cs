using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.TypeFinding;

namespace WebAppWithAppSettings
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().Implements<IOperator>().Tag("MathOperator"));
            TypeFinderOptions.Defaults.TypeFinderCriterias.Add(TypeFinderCriteriaBuilder.Create().Tag("All"));
            services.AddPluginFramework()
                .AddNugetConfiguration();
            
            services.AddControllers();
        }

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
