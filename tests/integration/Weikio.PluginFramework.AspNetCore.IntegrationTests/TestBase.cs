using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PluginFrameworkTestBed;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Samples.Shared;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.PluginFramework.AspNetCore.IntegrationTests
{
    public class TestBase : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        public ITestOutputHelper Output { get; set; }

        protected TestBase(WebApplicationFactory<Startup> factory, ITestOutputHelper output)
        {
            Output = output;
            _factory = factory;
        }  
        
        protected IServiceProvider Init(Action<IServiceCollection> action = null)
        {
            var folderPluginCatalog = new FolderPluginCatalog(@"..\..\..\..\..\..\samples\Shared\Weikio.PluginFramework.Samples.SharedPlugins\bin\Debug\net7.0".Replace(@"\",Path.DirectorySeparatorChar.ToString()), type =>
            {
                type.Implements<IOperator>();
            });
            
            var server = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddPluginFramework()
                        .AddPluginCatalog(folderPluginCatalog);
                    
                    action?.Invoke(services);
                });
                
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders(); // Remove other loggers
                    XUnitLoggerExtensions.AddXUnit((ILoggingBuilder) logging, Output); // Use the ITestOutputHelper instance
                });
                
            });

            return server.Services;
        }
    }
}
