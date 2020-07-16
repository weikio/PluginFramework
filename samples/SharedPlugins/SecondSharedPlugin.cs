using System;
using Shared;

namespace SharedPlugins
{
    public class SecondSharedPlugin : IOutPlugin
    {
        public string Get()
        {
            return "Second shared plugin";
        }
    }
}
