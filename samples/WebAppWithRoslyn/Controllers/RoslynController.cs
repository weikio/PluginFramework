using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;

namespace WebAppWithRoslyn.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RoslynController : ControllerBase
    {
        private readonly IPluginCatalog _pluginCatalog;
        private readonly IServiceProvider _sp;

        public RoslynController(IPluginCatalog pluginCatalog, IServiceProvider sp)
        {
            _pluginCatalog = pluginCatalog;
            _sp = sp;
        }
        
        [HttpGet]
        public async Task<string> Get()
        {
            var scriptPlugin = _pluginCatalog.GetPlugins().First();
                
            dynamic scriptInstance = Activator.CreateInstance(scriptPlugin);
            var scriptResult = await scriptInstance.Run();

            var typePlugin = _pluginCatalog.Get("MyPlugin", Version.Parse("1.5.0.0"));

            dynamic typeInstance = typePlugin.Create(_sp);
            
            var typeResult = typeInstance.RunThings();
            
            var result = new StringBuilder();
            result.AppendLine(scriptResult);
            result.AppendLine(typeResult);

            return result.ToString();
        }
    }
}
