using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Weikio.PluginFramework.Context;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalogOptions
    {
        public bool IncludSubfolders { get; set; } = true;
        public List<string> SearchPatterns = new List<string>() {"*.dll"};
        public List<Func<string, MetadataReader, TypeDefinition, bool>> PluginResolvers = new List<Func<string, MetadataReader, TypeDefinition, bool>>();
        // public List<Func<ITypeFindingContext, Type, bool>> TypePluginResolvers = new List<Func<ITypeFindingContext, Type, bool>>();
        public PluginLoadContextOptions PluginLoadContextOptions = new PluginLoadContextOptions();
        public TypeFinderCriteria TypeFinderCriteria = null;
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
    }
}
