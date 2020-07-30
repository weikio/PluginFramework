using System;
using Weikio.PluginFramework.Samples.Shared;

namespace ConsoleApp
{
    public class SecondPlugin : IPlugin
    {
        public void Run()
        {
            Console.WriteLine("Second plugin");
        }
    }
}