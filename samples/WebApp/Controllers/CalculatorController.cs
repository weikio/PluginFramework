using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.Abstractions;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly IEnumerable<IOperator> _operators;
        private readonly IEnumerable<Plugin> _plugins;
        private readonly IOperator _defaultOperator;

        public CalculatorController(IEnumerable<IOperator> operators, IEnumerable<Plugin> plugins, IOperator defaultOperator)
        {
            _operators = operators;
            _plugins = plugins;
            _defaultOperator = defaultOperator;
        }
        
        [HttpGet]
        public string Get()
        {
            return JsonSerializer.Serialize(new
            {
                allPlugins = _plugins.Select(p => p.ToString()),
                operators = _operators.Select(o => o.GetType().Name),
                defaultOperator = _defaultOperator.GetType().Name
            }, new JsonSerializerOptions { WriteIndented = true});
        }
    }
}
