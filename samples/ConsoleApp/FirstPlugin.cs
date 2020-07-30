using System;
using Weikio.PluginFramework.Samples.Shared;

namespace ConsoleApp
{
    public class FirstPlugin : IPlugin
    {
        public void Run()
        {
            Console.WriteLine("First plugin");
        }
    }
}