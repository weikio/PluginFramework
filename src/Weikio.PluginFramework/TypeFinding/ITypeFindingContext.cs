using System;
using System.Reflection;

namespace Weikio.PluginFramework.TypeFinding
{
    public interface ITypeFindingContext
    {
        Assembly FindAssembly(string assemblyName);
        Type FindType(Type type);
    }
}