using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs;
using Weikio.PluginFramework.Context;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class DefaultOptionsTests
    {
        [Fact]
        public async Task CanConfigureDefaultOptions()
        {
            throw new NotImplementedException();;
            //
            // // Make sure that the referenced version of JSON.NET is loaded into memory
            // var json = Newtonsoft.Json.JsonConvert.SerializeObject(1);
            //
            // PluginLoadContextOptions.Defaults.UseHostApplicationAssemblies = UseHostApplicationAssembliesEnum.Always;
            //
            // var assemblyPluginCatalog = new AssemblyPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonNew\netstandard2.0\JsonNetNew.dll");
            // await assemblyPluginCatalog.Initialize();
            //
            // var folderPluginCatalog = new FolderPluginCatalog(@"..\..\..\..\..\Assemblies\bin\JsonOld\netstandard2.0", new FolderPluginCatalogOptions()
            // {
            //     AssemblyPluginResolvers = new List<Func<Assembly, bool>>()
            //     {
            //         assembly =>
            //         {
            //             var result = assembly.ExportedTypes.Any(x => x.Name.EndsWith("JsonResolver"));
            //
            //             return result;
            //         }
            //     }
            // });
            //     
            // await folderPluginCatalog.Initialize();
            //
            // var assemblyPluginDefinition = (await assemblyPluginCatalog.GetAll()).Single();
            // var folderPluginDefinition = (await folderPluginCatalog.GetAll()).Single();
            //
            // var pluginExporter = new PluginExporter();
            // var newPlugin = await pluginExporter.Get(assemblyPluginDefinition);
            // var oldPlugin = await pluginExporter.Get(folderPluginDefinition);
            //
            // dynamic newPluginJsonResolver = Activator.CreateInstance(newPlugin.Types.First());
            // var newPluginVersion = newPluginJsonResolver.GetVersion();
            //
            // dynamic oldPluginJsonResolver = Activator.CreateInstance(oldPlugin.Types.First());
            // var oldPluginVersion = oldPluginJsonResolver.GetVersion();
            //
            // Assert.Equal("10.0.0.0", newPluginVersion);
            // Assert.Equal("10.0.0.0", oldPluginVersion);
        }
    }
}
