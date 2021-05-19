using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PluginFrameworkTestBed;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.Samples.SharedPlugins;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.PluginFramework.AspNetCore.IntegrationTests
{
    public class DefaultPluginTypeTests : TestBase
    {
        public DefaultPluginTypeTests(WebApplicationFactory<Startup> factory, ITestOutputHelper output) : base(factory, output)
        {
        }
        
        [Fact]
        public void DefaultsToFirstPluginType()
        {
            var sp = Init(services =>
            {
                services.AddPluginType<IOperator>();
            });

            var operators = sp.GetRequiredService<IEnumerable<IOperator>>();
            var defaultOperator = sp.GetRequiredService<IOperator>();
            
            Assert.Equal(operators.First().GetType(), defaultOperator.GetType());
        }
        
        [Fact]
        public void CanConfigureUsingAction()
        {
            var sp = Init(services =>
            {
                services.AddPluginType<IOperator>(configureDefault: option =>
                {
                    option.DefaultType = (provider, types) => typeof(SumOperator);
                });
            });

            var defaultOperator = sp.GetRequiredService<IOperator>();
            
            Assert.Equal(typeof(SumOperator), defaultOperator.GetType());
        }
        
        [Fact]
        public void CanConfigureUsingOptions()
        {
            var sp = Init(services =>
            {
                services.AddPluginType<IOperator>();
                
                services.Configure<DefaultPluginOption>(nameof(IOperator), option =>
                    option.DefaultType = (serviceProvider, implementingTypes) => typeof(MultiplyOperator));
            });

            var defaultOperator = sp.GetRequiredService<IOperator>();
            
            Assert.Equal(typeof(MultiplyOperator), defaultOperator.GetType());
        }
    }
}
