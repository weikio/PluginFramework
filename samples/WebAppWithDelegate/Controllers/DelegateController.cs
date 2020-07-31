using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;

namespace WebAppWithDelegate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DelegateController : ControllerBase
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IServiceProvider _sp;

        public DelegateController(IPluginCatalog pluginCatalog, IServiceProvider serviceProvider)
        {
            _pluginCatalog = pluginCatalog;
            _sp = serviceProvider;
        }

        [HttpGet]
        public string Get()
        {
            var actionPlugin = _pluginCatalog.Get("MyActionPlugin", Version.Parse("1.0.0.0"));
            var funcPlugin = _pluginCatalog.Get("MyFuncPlugin", Version.Parse("1.0.0.0"));
            var funcExternalServicePlugin = _pluginCatalog.Get("MyExternalServicePlugin", Version.Parse("1.0.0.0"));

            dynamic action = _sp.Create(actionPlugin);
            dynamic func = _sp.Create(funcPlugin);
            dynamic external = _sp.Create(funcExternalServicePlugin);

            var s = new List<string>() { "Hello from controller" };
            action.Run(s);

            var result = func.Run(s);
            
            // Conversion rules are used to convert the func's string parameter into a property and to convert the ExternalService into a constructor parameter.
            external.S = result;
            result = external.Run();

            return result;
        }
    }
}
