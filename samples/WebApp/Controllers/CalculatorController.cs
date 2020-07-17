using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Weikio.PluginFramework.Abstractions;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly List<IOperator> _operators;
        private readonly IEnumerable<Plugin> _plugins;

        public CalculatorController(List<IOperator> operators, IEnumerable<Plugin> plugins)
        {
            _operators = operators;
            _plugins = plugins;
        }
        
        [HttpGet]
        public string Get()
        {
            var pluginsList = string.Join(", ", _plugins);

            return pluginsList;
        }
    }
}
