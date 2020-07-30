using System;
using System.Threading.Tasks;
using Weikio.PluginFramework.Samples.Shared;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await AssemblyCatalogSample();
            await TypeCatalogSample();
            await CompositeCatalogSample();
        }

        private static async Task AssemblyCatalogSample()
        {
            Console.WriteLine("Assembly catalog sample");
            
            // 1. Create a new plugin catalog from the current assembly
            var assemblyPluginCatalog = new AssemblyPluginCatalog(typeof(Program).Assembly, type => typeof(IPlugin).IsAssignableFrom(type));

            // 2. Initialize the catalog
            await assemblyPluginCatalog.Initialize();

            // 3. Get the plugins from the catalog 
            var assemplyPlugins = assemblyPluginCatalog.GetPlugins();
            
            foreach (var plugin in assemplyPlugins)
            {
                var inst = (IPlugin) Activator.CreateInstance(plugin);
                inst.Run();
            }
        }
        
        private static async Task TypeCatalogSample()
        {
            Console.WriteLine("Type catalog sample");
            
            var typePluginCatalog = new TypePluginCatalog(typeof(FirstPlugin));
            await typePluginCatalog.Initialize();

            var typePlugin = typePluginCatalog.Get();
            
            var pluginInstance = (IPlugin) Activator.CreateInstance(typePlugin);
            pluginInstance.Run();
        }
        
        private static async Task CompositeCatalogSample()
        {
            Console.WriteLine("Composite catalog sample");
            
            // 1. Create a new plugin catalog from the current assembly
            var assemblyPluginCatalog = new AssemblyPluginCatalog(typeof(Program).Assembly, type => typeof(IPlugin).IsAssignableFrom(type));

            // 2. Also create a new plugin catalog from a type
            var typePluginCatalog = new TypePluginCatalog(typeof(MyPlugin));
            
            // 3. Then combine the catalogs into a composite catalog
            var compositeCatalog = new CompositePluginCatalog(assemblyPluginCatalog, typePluginCatalog);
            
            // 4. Initialize the composite catalog
            await compositeCatalog.Initialize();

            // 5. Get the plugins from the catalog 
            var assemplyPlugins = compositeCatalog.GetPlugins();
            
            foreach (var plugin in assemplyPlugins)
            {
                if (plugin.Type.Name == "MyPlugin")
                {
                    var inst = (IMyPlugin) Activator.CreateInstance(plugin);
                    inst.Run();
                }
                else
                {
                    var inst = (IPlugin) Activator.CreateInstance(plugin);
                    inst.Run();
                }
            }
        }
    }
}
