using System;
using Weikio.PluginFramework.Samples.Shared;

namespace Weikio.PluginFramework.Samples.SharedPlugins
{
    public class SecondSharedPlugin : IOutPlugin
    {
        public string Get()
        {
            return "Second shared plugin";
        }
    }
}
