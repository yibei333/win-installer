using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDevLib;
using SharpDevLib.Compression;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WinInstaller.Extensions;

namespace WinInstaller.Pages
{
    public partial class InstallPage : UserControl
    {
        public InstallViewModel ViewModel { get; set; } = new InstallViewModel();

        public InstallPage()
        {
            InitializeComponent();
        }
    }

    public partial class InstallViewModel : ObservableObject
    {
        public InstallViewModel()
        {
            Location = RegistryExtension.Get();
            if (Location.IsNullOrWhiteSpace()) Location = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!Location.EndsWith(App.Name)) Location = Location.CombinePath(App.Name);
        }

        [ObservableProperty]
        string _log;
        [ObservableProperty]
        double _progress;

        [ObservableProperty]
        string _location;

        string ExePath => Location.CombinePath(App.EntryPoint);

        [RelayCommand]
        async Task SelectPath()
        {
            await Task.Yield();
            var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "选择安装位置" };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var path = dialog.SelectedPath.FormatPath().TrimEnd("/");
                if (!path.EndsWith(App.Name)) path = path.CombinePath(App.Name);
                Location = path;
            }
        }

        [RelayCommand]
        async Task Install()
        {
            MainWindow.Instance.ViewModel.Running = true;
            Progress = 0;
            Log = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(Location))
                {
                    MessageBox.Show("请选择安装位置");
                    return;
                }
                Location = Location.FormatPath();
                Location.CreateDirectoryIfNotExist();
                WriteRegistry();
                if (!StopRunningApp()) return;
                await CopyFiles();
                CreateShortcut();

                MainWindow.Instance.SetPage<CompletePage>();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            MainWindow.Instance.ViewModel.Running = false;
        }

        void WriteRegistry()
        {
            RegistryExtension.Set(Location);
        }

        bool StopRunningApp()
        {
            var processes = Process.GetProcessesByName(App.Name).Where(x => x.MainModule.FileName.FormatPath() == ExePath).ToList();
            if (!processes.Any()) return true;

            if (MessageBox.Show("确定停止正在运行的程序?", "消息确认", System.Windows.MessageBoxButton.OKCancel) == System.Windows.MessageBoxResult.OK)
            {
                foreach (var process in processes)
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }
                return true;
            }
            else return false;
        }

        async Task CopyFiles()
        {
            double size = 0;
            var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("Source.zip") ?? throw new Exception("找不到源程序");
            var zipPath = AppDomain.CurrentDomain.BaseDirectory.CombinePath("Source.zip");
            stream.SaveToFile(zipPath);
            stream.Close();

            var lastName = string.Empty;
            await new DeCompressOption(zipPath, Location)
            {
                OnProgress = (e) =>
                {
                    if (lastName == e.CurrentName) return;
                    lastName = e.CurrentName;
                    Log += $"复制文件:{e.CurrentName}\r\n";
                    Progress = e.Progress;
                    size = e.Total;
                },
            }.DeCompressAsync();
            RegistryExtension.SetSystemSizeInformation((int)size / (1024));

#if DEBUG
            await Task.Delay(3000);
#endif

            //todo
            //upgrader
            //uninstaller
        }

        void CreateShortcut()
        {
            var path = ShortcutExtension.CreateShortcutToDesktop(Location);
            var name = path.GetFileName();

            //desktop
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory).CombinePath(name);
            File.Copy(path, desktopPath, true);

            //user start
            //user start path->C:\Users\{user}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs
            //system start path->C:\ProgramData\Microsoft\Windows\Start Menu\Programs
            var userStartPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu).CombinePath($"Programs/{name}");
            File.Copy(path, userStartPath, true);

            path.RemoveFileIfExist();
        }
    }
}
