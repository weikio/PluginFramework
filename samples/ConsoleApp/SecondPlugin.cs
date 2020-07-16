using System;
using Shared;

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