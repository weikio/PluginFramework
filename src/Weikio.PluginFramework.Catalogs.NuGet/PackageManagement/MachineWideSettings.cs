using System;
using System.Collections.Generic;
using System.Text;
using NuGet.Common;
using NuGet.Configuration;

namespace Weikio.PluginFramework.Catalogs.NuGet.PackageManagement
{
    public class MachineWideSettings : IMachineWideSettings
    {
        private readonly Lazy<ISettings> _settings;

        public MachineWideSettings()
        {
            _settings = new Lazy<ISettings>(() =>
            {
                var baseDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.MachineWideConfigDirectory);
                return global::NuGet.Configuration.Settings.LoadMachineWideSettings(baseDirectory);
            });
        }

        public ISettings Settings => _settings.Value;
    }
}
