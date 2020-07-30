using System;
using Weikio.PluginFramework.Samples.Shared;

namespace ConsoleApp
{
    public class MyPlugin : IMyPlugin
    {
        public void Run()
        {
            Console.WriteLine("My plugin which implements IMyPlugin");
        }
    }
}