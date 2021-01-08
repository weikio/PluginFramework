using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Weikio.PluginFramework.Abstractions.DependencyInjection
{
    public interface IPluginFrameworkBuilder
    {
        IServiceCollection Services { get; }
    }
}
