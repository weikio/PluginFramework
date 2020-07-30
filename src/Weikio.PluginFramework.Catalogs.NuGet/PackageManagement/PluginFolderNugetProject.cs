using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Weikio.PluginFramework.Catalogs.NuGet.PackageManagement
{
    public class PluginFolderNugetProject : FolderNuGetProject
    {
        private const string PluginAssemblyFilesFileName = "pluginAssemblyFiles.txt";

        private readonly string _root;
        private readonly IPackageSearchMetadata _pluginNuGetPackage;
        private readonly NuGetFramework _targetFramework;
        private readonly bool _onlyDownload;

        public PluginFolderNugetProject(string root, IPackageSearchMetadata pluginNuGetPackage, NuGetFramework targetFramework, bool onlyDownload = false) :
            base(root, new PackagePathResolver(root), targetFramework)
        {
            _root = root;
            _pluginNuGetPackage = pluginNuGetPackage;
            _targetFramework = targetFramework;
            _onlyDownload = onlyDownload;
        }

        public override async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(CancellationToken token)
        {
            return await base.GetInstalledPackagesAsync(token);
        }

        public override async Task<bool> InstallPackageAsync(PackageIdentity packageIdentity, DownloadResourceResult downloadResourceResult, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            var result = await base.InstallPackageAsync(packageIdentity, downloadResourceResult, nuGetProjectContext, token);

            if (_onlyDownload)
            {
                return result;
            }
            
            using (var zipArchive = new ZipArchive(downloadResourceResult.PackageStream))
            {
                var zipArchiveEntries = zipArchive.Entries
                    .Where(e => e.Name.EndsWith(".dll") || e.Name.EndsWith(".exe")).ToList();
            
                var entriesWithTargetFramework = zipArchiveEntries
                    .Select(e => new
                    {
                        TargetFramework = NuGetFramework.Parse(e.FullName.Split('/')[1]),
                        Entry = e
                    }).ToList();
                
                var matchingEntries = entriesWithTargetFramework
                    .Where(e => e.TargetFramework.Version.Major > 0 &&
                                e.TargetFramework.Version <= _targetFramework.Version).ToList();
            
                var orderedEntries = matchingEntries
                    .OrderBy(e => e.TargetFramework.GetShortFolderName()).ToList();
            
                if (orderedEntries.Any())
                {
                    var dllEntries = orderedEntries
                        .GroupBy(e => e.TargetFramework.GetShortFolderName())
                        .Last()
                        .Select(e => e.Entry)
                        .ToArray();
            
                    var pluginAssemblies = new List<string>();
            
                    foreach (var e in dllEntries)
                    {
                        e.ExtractToFile(Path.Combine(_root, e.Name), overwrite: true);
            
                        if (packageIdentity.Id == _pluginNuGetPackage.Identity.Id)
                        {
                            pluginAssemblies.Add(e.Name);
                        }
                    }
            
                    await File.WriteAllLinesAsync(Path.Combine(_root, PluginAssemblyFilesFileName), pluginAssemblies);
                }
            }

            return result;
        }

        public async Task<string[]> GetPluginAssemblyFilesAsync()
        {
            return await File.ReadAllLinesAsync(Path.Combine(_root, PluginAssemblyFilesFileName));
        }

        public override async Task<bool> UninstallPackageAsync(PackageIdentity packageIdentity, INuGetProjectContext nuGetProjectContext, CancellationToken token)
        {
            return await base.UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);
        }
    }
}
