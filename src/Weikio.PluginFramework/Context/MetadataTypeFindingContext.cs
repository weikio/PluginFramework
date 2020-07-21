using System;
using System.Reflection;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Context
{
    public class MetadataTypeFindingContext : ITypeFindingContext
    {
        private readonly MetadataLoadContext _metadataLoadContext;

        public MetadataTypeFindingContext(MetadataLoadContext metadataLoadContext)
        {
            _metadataLoadContext = metadataLoadContext;
        }

        public Assembly FindAssembly(string assemblyName)
        {
            var result = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

            return result;
        }

        public Type FindType(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            var assembly = _metadataLoadContext.LoadFromAssemblyName(assemblyName);

            var result = assembly.GetType(type.FullName);

            return result;
        }
    }
}