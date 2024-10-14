using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace WinInstaller.Updater
{
    public class Config
    {
        public const string Name = "SampleApp.WpfApp";
        static string RegistryKeyPath => $"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{Name}";

        public string DisplayName { get; set; }
        public string DisplayIcon => Path.Combine(InstallLocation, EntryPoint);
        public string DisplayVersion { get; set; }
        public string EntryPoint { get; set; }
        public string InstallLocation { get; set; }
        public string Publisher { get; set; }
        public string UninstallString => Path.Combine(InstallLocation, "Uninstaller.exe");
        public int EstimatedSize { get; set; }
        public int NoModify { get; set; } = 1;
        public int NoRepair { get; set; } = 1;
        public string VersionCheckUrl { get; set; }
        public string PackageDownloadUrl { get; set; }

        public static Config Get()
        {
            var config = new Config();
            var type = typeof(Config);
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key == null) return config;
            foreach (var name in key.GetValueNames())
            {
                var property = type.GetProperty(name);
                if (property.CanWrite) property?.SetValue(config, key.GetValue(name));
            }
            return config;
        }

        public void Set()
        {
            var type = typeof(Config);
            using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key == null) return;

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                key.SetValue(property.Name, property.GetValue(this)??string.Empty);
            }
        }

        public void Delete()
        {
            Registry.CurrentUser.DeleteSubKey(RegistryKeyPath);
        }
    }
}
