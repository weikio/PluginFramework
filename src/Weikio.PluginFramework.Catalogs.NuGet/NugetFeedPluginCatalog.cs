using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Weikio.NugetDownloader;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.TypeFinding;

// ReSharper disable once CheckNamespace
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
        private readonly NugetFeedPluginCatalogOptions _options;

        public string PackagesFolder { get; }

        public NugetFeedPluginCatalog(NuGetFeed packageFeed, string searchTerm = null,
            bool includePrereleases = false, int maxPackages = 128,
            string packagesFolder = null, Action<TypeFinderCriteriaBuilder> configureFinder = null, Dictionary<string, TypeFinderCriteria> criterias = null, 
            NugetFeedPluginCatalogOptions options = null)
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

            _options = options ?? new NugetFeedPluginCatalogOptions();

            if (configureFinder != null)
            {
                var builder = new TypeFinderCriteriaBuilder();
                configureFinder(builder);

                var criteria = builder.Build();

                _options.TypeFinderOptions.TypeFinderCriterias.Add(criteria);
            }
            
            foreach (var finderCriteria in criterias)
            {
                finderCriteria.Value.Tags = new List<string>() { finderCriteria.Key };
                
                _options.TypeFinderOptions.TypeFinderCriterias.Add(finderCriteria.Value);
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
            var nuGetDownloader = new NuGetDownloader(_options.LoggerFactory());

            var packages = await nuGetDownloader.SearchPackagesAsync(_packageFeed, _searchTerm, maxResults: _maxPackages);

            foreach (var packageAndRepo in packages)
            {
                var options = new NugetPluginCatalogOptions() { TypeFinderOptions = _options.TypeFinderOptions, PluginNameOptions = _options.PluginNameOptions};
                
                var packageCatalog = new NugetPackagePluginCatalog(packageAndRepo.Package.Identity.Id, packageAndRepo.Package.Identity.Version.ToString(),
                    _includePrereleases, _packageFeed, PackagesFolder, options: options);

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
