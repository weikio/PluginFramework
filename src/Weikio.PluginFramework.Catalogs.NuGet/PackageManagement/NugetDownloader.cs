using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.PackageExtraction;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Weikio.PluginFramework.Catalogs.NuGet.PackageManagement
{
    public class NuGetDownloader
    {
        private readonly ILogger _logger;

        public NuGetDownloader()
        {
            _logger = new ConsoleLogger();
        }

  public async Task<string[]> DownloadAsync(string packageFolder, string packageName, string packageVersion = null, bool includePrerelease = false,
            NuGetFeed packageFeed = null, bool onlyDownload = false)
        {
            if (!Directory.Exists(packageFolder))
            {
                Directory.CreateDirectory(packageFolder);
            }

            var providers = GetNugetResourceProviders();
            var settings = Settings.LoadDefaultSettings(packageFolder, null, new MachineWideSettings());
            var packageSourceProvider = new PackageSourceProvider(settings);
            var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, providers);

            var dotNetFramework = Assembly
                .GetEntryAssembly()
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            var frameworkNameProvider = new FrameworkNameProvider(
                new[] { DefaultFrameworkMappings.Instance },
                new[] { DefaultPortableFrameworkMappings.Instance });

            var nuGetFramework = NuGetFramework.ParseFrameworkName(dotNetFramework, frameworkNameProvider);

            IPackageSearchMetadata package = null;
            SourceRepository sourceRepo = null;

            if (!string.IsNullOrWhiteSpace(packageFeed?.Feed))
            {
                sourceRepo = GetSourceRepo(packageFeed, providers);

                package = await SearchPackageAsync(packageName, packageVersion, includePrerelease, sourceRepo);
            }
            else
            {
                foreach (var repo in sourceRepositoryProvider.GetRepositories())
                {
                    if (packageFeed != null && repo.PackageSource.Name != packageFeed.Name)
                    {
                        continue;
                    }

                    package = await SearchPackageAsync(packageName, packageVersion, includePrerelease, repo);

                    if (package != null)
                    {
                        sourceRepo = repo;

                        break;
                    }
                }
            }

            if (package == null)
            {
                throw new PackageNotFoundException($"Couldn't find package '{packageVersion}'.{packageVersion}.");
            }

            var project = new PluginFolderNugetProject(packageFolder, package, nuGetFramework, onlyDownload);
            var packageManager = new NuGetPackageManager(sourceRepositoryProvider, settings, packageFolder) { PackagesFolderNuGetProject = project };

            var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, _logger);

            var projectContext = new FolderProjectContext(_logger)
            {
                PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv2,
                    PackageExtractionBehavior.XmlDocFileSaveMode,
                    clientPolicyContext,
                    _logger)
            };

            var resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest,
                includePrerelease,
                includeUnlisted: false,
                VersionConstraints.None);

            var downloadContext = new PackageDownloadContext(
                resolutionContext.SourceCacheContext,
                packageFolder,
                resolutionContext.SourceCacheContext.DirectDownload);

            // We are waiting here instead of await as await actually doesn't seem to work correctly.
            packageManager.InstallPackageAsync(
                project,
                package.Identity,
                resolutionContext,
                projectContext,
                downloadContext,
                sourceRepo,
                new List<SourceRepository>(),
                CancellationToken.None).Wait();

            if (onlyDownload)
            {
                var versionFolder = Path.Combine(packageFolder, package.Identity.ToString());

                return Directory.GetFiles(versionFolder, "*.*", SearchOption.AllDirectories);
            }

            return await project.GetPluginAssemblyFilesAsync();
        }

        public async Task<string[]> DownloadAsync(IPackageSearchMetadata packageIdentity, SourceRepository repository,
            string downloadFolder, bool onlyDownload = false)
        {
            if (packageIdentity == null)
            {
                throw new ArgumentNullException(nameof(packageIdentity));
            }

            var providers = GetNugetResourceProviders();
            var settings = Settings.LoadDefaultSettings(downloadFolder, null, new MachineWideSettings());
            var packageSourceProvider = new PackageSourceProvider(settings);
            var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, providers);

            var dotNetFramework = Assembly
                .GetEntryAssembly()
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName;

            var frameworkNameProvider = new FrameworkNameProvider(
                new[] { DefaultFrameworkMappings.Instance },
                new[] { DefaultPortableFrameworkMappings.Instance });

            var nuGetFramework = NuGetFramework.ParseFrameworkName(dotNetFramework, frameworkNameProvider);

            var project = new PluginFolderNugetProject(downloadFolder, packageIdentity, nuGetFramework, onlyDownload);
            
            var packageManager = new NuGetPackageManager(sourceRepositoryProvider, settings, downloadFolder) { PackagesFolderNuGetProject = project };

            var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, _logger);

            var projectContext = new FolderProjectContext(_logger)
            {
                PackageExtractionContext = new PackageExtractionContext(
                    PackageSaveMode.Defaultv2,
                    PackageExtractionBehavior.XmlDocFileSaveMode,
                    clientPolicyContext,
                    _logger)
            };

            var resolutionContext = new ResolutionContext(
                DependencyBehavior.Lowest,
                true,
                includeUnlisted: false,
                VersionConstraints.None);

            var downloadContext = new PackageDownloadContext(
                resolutionContext.SourceCacheContext,
                downloadFolder,
                resolutionContext.SourceCacheContext.DirectDownload);

            await packageManager.InstallPackageAsync(
                project,
                packageIdentity.Identity,
                resolutionContext,
                projectContext,
                downloadContext,
                repository,
                new List<SourceRepository>(),
                CancellationToken.None);

            var versionFolder = Path.Combine(downloadFolder, packageIdentity.Identity.ToString());

            return Directory.GetFiles(versionFolder, "*.*", SearchOption.AllDirectories);
        }

        private static List<Lazy<INuGetResourceProvider>> GetNugetResourceProviders()
        {
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3()); // Add v3 API s

            return providers;
        }

        private static SourceRepository GetSourceRepo(NuGetFeed packageFeed, List<Lazy<INuGetResourceProvider>> providers)
        {
            SourceRepository sourceRepo;
            var packageSource = new PackageSource(packageFeed.Feed);

            if (!string.IsNullOrWhiteSpace(packageFeed.Username))
            {
                packageSource.Credentials = new PackageSourceCredential(packageFeed.Name, packageFeed.Username, packageFeed.Password, true, null);
            }

            sourceRepo = new SourceRepository(packageSource, providers);

            return sourceRepo;
        }

        public async IAsyncEnumerable<(SourceRepository Repository, IPackageSearchMetadata Package)> SearchPackagesAsync(string searchTerm, int maxResults = 128, 
            bool includePrerelease = false,
            string nugetConfigFilePath = "")
        {
            var providers = GetNugetResourceProviders();

            var packageRootFolder = "";

            if (!string.IsNullOrWhiteSpace(Assembly.GetEntryAssembly()?.Location))
            {
                packageRootFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            }

            ISettings settings;

            if (!string.IsNullOrWhiteSpace(nugetConfigFilePath))
            {
                settings = Settings.LoadSettingsGivenConfigPaths(new List<string>() { nugetConfigFilePath });
            }
            else
            {
                settings = Settings.LoadDefaultSettings(packageRootFolder ?? "", null, new MachineWideSettings());
            }

            var packageSourceProvider = new PackageSourceProvider(settings);
            var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, providers);

            var repositories = sourceRepositoryProvider.GetRepositories();

            foreach (var repository in repositories)
            {
                var packageSearchResource = await repository.GetResourceAsync<PackageSearchResource>();

                SearchFilter searchFilter;

                if (includePrerelease)
                {
                    searchFilter = new SearchFilter(includePrerelease, SearchFilterType.IsAbsoluteLatestVersion);
                }
                else
                {
                    searchFilter = new SearchFilter(includePrerelease);
                }

                var items = await packageSearchResource.SearchAsync(searchTerm, searchFilter, 0, maxResults, _logger, CancellationToken.None);

                foreach (var packageSearchMetadata in items)
                {
                    yield return (repository, packageSearchMetadata);
                }
            }
        }

        public async Task<IEnumerable<(SourceRepository Repository, IPackageSearchMetadata Package)>> SearchPackagesAsync(NuGetFeed packageFeed, string searchTerm, int maxResults = 128,
            bool includePrerelease = false)
        {
            var providers = GetNugetResourceProviders();
            var sourceRepo = GetSourceRepo(packageFeed, providers);
            var packageSearchResource = await sourceRepo.GetResourceAsync<PackageSearchResource>();

            SearchFilter searchFilter;

            if (includePrerelease)
            {
                searchFilter = new SearchFilter(includePrerelease, SearchFilterType.IsAbsoluteLatestVersion);
            }
            else
            {
                searchFilter = new SearchFilter(includePrerelease);
            }
            
            var packages = await packageSearchResource.SearchAsync(searchTerm, searchFilter, 0, maxResults, _logger, CancellationToken.None);

            return packages.Select(x => (sourceRepo, x));
        }

        private async Task<IPackageSearchMetadata> SearchPackageAsync(string packageName, string version, bool includePrerelease,
            SourceRepository sourceRepository)
        {
            var packageMetadataResource = await sourceRepository.GetResourceAsync<PackageMetadataResource>();
            var sourceCacheContext = new SourceCacheContext();

            IPackageSearchMetadata packageMetaData = null;

            if (!string.IsNullOrEmpty(version) && !version.Contains('*'))
            {
                if (NuGetVersion.TryParse(version, out var nugetversion))
                {
                    var packageIdentity = new PackageIdentity(packageName, NuGetVersion.Parse(version));

                    packageMetaData = await packageMetadataResource.GetMetadataAsync(
                        packageIdentity,
                        sourceCacheContext,
                        _logger,
                        CancellationToken.None);
                }
            }
            else
            {
                var searchResults = await packageMetadataResource.GetMetadataAsync(
                    packageName,
                    includePrerelease,
                    includeUnlisted: false,
                    sourceCacheContext,
                    _logger,
                    CancellationToken.None);

                searchResults = searchResults
                    .OrderByDescending(p => p.Identity.Version);

                if (!string.IsNullOrEmpty(version))
                {
                    var searchPattern = version.Replace("*", ".*");
                    searchResults = searchResults.Where(p => Regex.IsMatch(p.Identity.Version.ToString(), searchPattern));
                }

                packageMetaData = searchResults.FirstOrDefault();
            }

            return packageMetaData;
        }
    }
}
