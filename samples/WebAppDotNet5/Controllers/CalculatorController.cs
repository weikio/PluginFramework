using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Samples.Shared;

namespace WebAppDotNet5.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly IEnumerable<IOperator> _operators;
        private readonly IEnumerable<Plugin> _plugins;

        public CalculatorController(IEnumerable<IOperator> operators, IEnumerable<Plugin> plugins, IOperator myOperator)
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
