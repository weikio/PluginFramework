using System.Reflection;
using System.Runtime.Loader;

namespace Weikio.PluginFramework.Catalogs.Delegates
{
    public sealed class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
}