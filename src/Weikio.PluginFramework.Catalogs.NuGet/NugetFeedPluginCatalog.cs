using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs.NuGet.PackageManagement;
using Weikio.PluginFramework.TypeFinding;

namespace Weikio.PluginFramework.Catalogs
{
    public class NugetFeedPluginCatalog : IPluginCatalog
    {
        private readonly NuGetFeed _packageFeed;
        private readonly string _searchTerm;
        private readonly int _maxPackages;
        private readonly bool _includePrereleases;

        private readonly HashSet<string> _pluginAssemblyNames = new HashSet<string>();

        private List<NugetPackagePluginCatalog> _pluginCatalogs = new List<NugetPackagePluginCatalog>();
        private Dictionary<string, TypeFinderCriteria> _typeFinderCriterias;
        
        public string PackagesFolder { get; }

        public NugetFeedPluginCatalog(NuGetFeed packageFeed, string searchTerm = null, 
            bool includePrereleases = false, int maxPackages = 128,  
            string packagesFolder = null, Action<TypeFinderCriteriaBuilder> configureFinder = null, Dictionary<string, TypeFinderCriteria> criterias = null)
        {
            _packageFeed = packageFeed;
            _searchTerm = searchTerm;
            _includePrereleases = includePrereleases;
            _maxPackages = maxPackages;

            PackagesFolder = packagesFolder ?? Path.Combine(Path.GetTempPath(), "NugetFeedPluginCatalog", Path.GetRandomFileName());

            if (!Directory.Exists(PackagesFolder))
            {
                Directory.CreateDirectory(PackagesFolder);
            }

            if (criterias == null)
            {
                criterias = new Dictionary<string, TypeFinderCriteria>();
            }

            _typeFinderCriterias = criterias;

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var criteria = builder.Build();

                _typeFinderCriterias.Add("", criteria);
            }
        }

        Plugin IPluginCatalog.Get(string name, Version version)
        {
            foreach (var pluginCatalog in _pluginCatalogs)
            {
                var result = pluginCatalog.Get(name, version);

                if (result == null)
                {
                    continue;
                }

                return result;
            }
            
            return null;
        }

        public bool IsInitialized { get; private set; }

        public async Task Initialize()
        {
            var nuGetDownloader = new NuGetDownloader();

            var packages = await nuGetDownloader.SearchPackagesAsync(_packageFeed, _searchTerm, maxResults: _maxPackages);

            foreach (var packageAndRepo in packages)
            {
                var packageCatalog = new NugetPackagePluginCatalog(packageAndRepo.Package.Identity.Id, packageAndRepo.Package.Identity.Version.ToString(), 
                    _includePrereleases, _packageFeed, PackagesFolder, criterias: _typeFinderCriterias);

                await packageCatalog.Initialize();
                
                _pluginCatalogs.Add(packageCatalog);
            }

            IsInitialized = true;
        }

        public List<Plugin> GetPlugins()
        {
            return _pluginCatalogs.SelectMany(x => x.GetPlugins()).ToList();
        }
    }
}
