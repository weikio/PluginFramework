using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Catalogs.Roslyn.Tests
{
    public static class TestHelpers
    {
        public static async Task<List<Type>> CompileScript(string code, RoslynPluginCatalogOptions options = null)
        {
            var catalog = new ScriptInitializer(code, options);
            var assembly = await catalog.CreateAssembly();

            var result = assembly.GetTypes().Where(x => x.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null).ToList();

            return result;
        }

        public static async Task<List<Type>> CompileRegular(string code, RoslynPluginCatalogOptions options = null)
        {
            var catalog = new RegularInitializer(code, options);
            var assembly = await catalog.CreateAssembly();

            var result = assembly.GetTypes().Where(x => x.GetCustomAttribute(typeof(CompilerGeneratedAttribute), true) == null).ToList();

            return result;
        }

        public static async Task<List<Type>> CreateCatalog(string code, RoslynPluginCatalogOptions options = null)
        {
            var catalog = new RoslynPluginCatalog(code, options);
            await catalog.Initialize();

            return catalog.GetPlugins().Select(x => x.Type).ToList();
        }
    }
}
