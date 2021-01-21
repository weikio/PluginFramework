using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions.DependencyInjection;

namespace Weikio.PluginFramework.AspNetCore
{
    public class PluginFrameworkBuilder : IPluginFrameworkBuilder
    {
        public PluginFrameworkBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}
