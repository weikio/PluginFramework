using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Weikio.NugetDownloader;
using Weikio.PluginFramework.Abstractions;
using Weikio.PluginFramework.Catalogs.NuGet;
using Weikio.PluginFramework.Context;
using Weikio.PluginFramework.TypeFinding;

// ReSharper disable once CheckNamespace
namespace Weikio.PluginFramework.Catalogs
{
    public class NugetPackagePluginCatalog : IPluginCatalog
    {
        private readonly NuGetFeed _packageFeed;
        private readonly string _packageName;
        private readonly string _packageVersion;
        private readonly bool _includePrerelease;

        private readonly HashSet<string> _pluginAssemblyFilePaths = new HashSet<string>();
        private readonly List<AssemblyPluginCatalog> _pluginCatalogs = new List<AssemblyPluginCatalog>();
        private readonly NugetPluginCatalogOptions _options;

        public string PackagesFolder { get; }

        private bool HasCustomPackagesFolder
        {
            get
            {
                return string.IsNullOrWhiteSpace(PackagesFolder) == false;
            }
        }
        
        private bool ForcePackageCache
        {
            get
            {
                if (HasCustomPackagesFolder == false)
                {
                    return false;
                }

                if (_options?.ForcePackageCaching != true)
                {
                    return false;
                }

                return true;
            }
        }

        private string NugetDownloadResultFilePath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PackagesFolder))
                {
                    return null;
                }

                var result = Path.Combine(PackagesFolder, ".nugetDownloadResult.json");

                return result;
            }
        }

        public NugetPackagePluginCatalog(string packageName, string packageVersion = null, bool includePrerelease = false, NuGetFeed packageFeed = null,
            string packagesFolder = null, Action<TypeFinderCriteriaBuilder> configureFinder = null, Dictionary<string, TypeFinderCriteria> criterias = null,
            NugetPluginCatalogOptions options = null)
        {
            _packageName = packageName;
            _packageVersion = packageVersion;
            _includePrerelease = includePrerelease;
            _packageFeed = packageFeed;

            PackagesFolder = packagesFolder ?? options?.CustomPackagesFolder;

            if (string.IsNullOrWhiteSpace(PackagesFolder))
            {
                PackagesFolder = Path.Combine(Path.GetTempPath(), "NugetPackagePluginCatalog", Path.GetRandomFileName());
            }

            if (!Directory.Exists(PackagesFolder))
            {
                Directory.CreateDirectory(PackagesFolder);
            }

            if (criterias == null)
            {
                criterias = new Dictionary<string, TypeFinderCriteria>();
            }

            _options = options ?? new NugetPluginCatalogOptions();

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

        public bool IsInitialized { get; private set; }

        public List<Plugin> GetPlugins()
        {
            return _pluginCatalogs.SelectMany(x => x.GetPlugins()).ToList();
        }

        public Plugin Get(string name, Version version)
        {
            foreach (var assemblyPluginCatalog in _pluginCatalogs)
            {
                var result = assemblyPluginCatalog.Get(name, version);

                if (result == null)
                {
                    continue;
                }

                return result;
            }

            return null;
        }

        public async Task Initialize()
        {
            NugetDownloadResult nugetDownloadResult = null;

            var logger = _options.LoggerFactory();

            if (ForcePackageCache && File.Exists(NugetDownloadResultFilePath))
            {
                try
                {
                    var jsonFromDisk = await File.ReadAllTextAsync(NugetDownloadResultFilePath);

                    nugetDownloadResult = JsonConvert.DeserializeObject<NugetDownloadResult>(jsonFromDisk);
                    
                    logger?.LogDebug($"Using previously downloaded package from {PackagesFolder}");
                }
                catch (Exception e)
                {
                    logger?.LogError($"Failed to deserialize nuget download result from path {NugetDownloadResultFilePath}: {e}");
                }
            }

            if (nugetDownloadResult == null)
            {
                var nuGetDownloader = new NuGetDownloader(_options.LoggerFactory());

                nugetDownloadResult = await nuGetDownloader.DownloadAsync(PackagesFolder, _packageName, _packageVersion, _includePrerelease, _packageFeed,
                    includeSecondaryRepositories: _options.IncludeSystemFeedsAsSecondary, targetFramework: _options.TargetFramework).ConfigureAwait(false);
            }

            foreach (var f in nugetDownloadResult.PackageAssemblyFiles)
            {
                _pluginAssemblyFilePaths.Add(Path.Combine(PackagesFolder, f));
            }

            foreach (var pluginAssemblyFilePath in _pluginAssemblyFilePaths)
            {
                var options = new AssemblyPluginCatalogOptions
                {
                    TypeFinderOptions = _options.TypeFinderOptions,
                    PluginNameOptions = _options.PluginNameOptions
                };

                var downloadedRuntimeDlls = nugetDownloadResult.RunTimeDlls.Where(x => x.IsRecommended).ToList();

                var runtimeAssemblyHints = new List<RuntimeAssemblyHint>();

                foreach (var runTimeDll in downloadedRuntimeDlls)
                {
                    var runtimeAssembly = new RuntimeAssemblyHint(runTimeDll.FileName, runTimeDll.FullFilePath, runTimeDll.IsNative);
                    runtimeAssemblyHints.Add(runtimeAssembly);
                }

                options.PluginLoadContextOptions.RuntimeAssemblyHints = runtimeAssemblyHints;

                var assemblyCatalog = new AssemblyPluginCatalog(pluginAssemblyFilePath, options);
                await assemblyCatalog.Initialize();

                _pluginCatalogs.Add(assemblyCatalog);
            }

            IsInitialized = true;

            if (ForcePackageCache)
            {
                try
                {
                    var jsonToWrite = JsonConvert.SerializeObject(nugetDownloadResult, Formatting.Indented);

                    await File.WriteAllTextAsync(NugetDownloadResultFilePath, jsonToWrite);
                    
                    logger?.LogDebug($"Stored downloaded package details to {NugetDownloadResultFilePath}");
                }
                catch (Exception e)
                {
                    logger?.LogError($"Failed to store downloaded package details to {NugetDownloadResultFilePath}: {e}");
                }
            }
        }
    }
}
