using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    public class FolderPluginCatalogOptions
    {
        public bool IncludSubfolders { get; set; } = true;
        public List<string> SearchPatterns = new List<string>() {"*.dll"};
        // public List<Func<string, MetadataReader, TypeDefinition, bool>> PluginResolvers = new List<Func<string, MetadataReader, TypeDefinition, bool>>();
        public PluginLoadContextOptions PluginLoadContextOptions = new PluginLoadContextOptions();
        public TypeFinderCriteria TypeFinderCriteria = null;
        public Dictionary<string, TypeFinderCriteria> TypeFinderCriterias = new Dictionary<string, TypeFinderCriteria>();
        public PluginNameOptions PluginNameOptions { get; set; } = new PluginNameOptions();
    }
}
