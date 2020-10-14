using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.AspNetCore;
using Weikio.PluginFramework.Samples.Shared;

namespace WebAppWithAppSettings.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly IEnumerable<Plugin> _plugins;
        private readonly IServiceProvider _serviceProvider;
        private readonly PluginProvider _pluginProvider;

        public CalculatorController(IEnumerable<Plugin> plugins, IServiceProvider serviceProvider, PluginProvider pluginProvider)
        {
            _plugins = plugins;
            _serviceProvider = serviceProvider;
            _pluginProvider = pluginProvider;
        }
        
        [HttpGet]
        public string Get()
        {
            var result = new StringBuilder();

            result.AppendLine("All:");
            foreach (var plugin in _plugins)
            {
                result.AppendLine($"{plugin.Name}: {plugin.Version}, Tags: {string.Join(", ", plugin.Tags)}");
            }

            var mathPlugins = _pluginProvider.GetByTag("MathOperator");
            var value1 = 10;
            var value2 = 20;

            result.AppendLine($"Math operations with values {value1} and {value2}");

            foreach (var mathPlugin in mathPlugins)
            {
                var mathPluginInstance = _serviceProvider.Create<IOperator>(mathPlugin);
                
                var mathResult = mathPluginInstance.Calculate(value1, value2);
                result.AppendLine($"{mathPlugin.Name}: {mathResult}");
            }
            
            return result.ToString();
        }
    }
}
