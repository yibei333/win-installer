using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDevLib;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WinInstaller.Pages
{
    /// <summary>
    /// Interaction logic for LicensePage.xaml
    /// </summary>
    public partial class PreRequestPage : Page
    {
        public PreRequestViewModel ViewModel { get; set; } = new PreRequestViewModel();

        public PreRequestPage()
        {
            InitializeComponent();

            if (ViewModel.Data.IsNullOrEmpty()) NextPage();
        }

        async void NextPage()
        {
            await Task.Yield();
            MainWindow.Instance.SetPage<InstallPage>();
        }
    }

    public partial class PreRequestViewModel : ObservableObject
    {
        public PreRequestViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var index = 0;
            assembly.GetManifestResourceNames().Where(x => x.StartsWith("PreRequests\\")).ForEach(x =>
            {
                index++;
                var stream = assembly.GetManifestResourceStream(x);
                var targetPath = AppDomain.CurrentDomain.BaseDirectory.CombinePath(x.TrimStart("PreRequests"));
                new FileInfo(targetPath).DirectoryName.CreateDirectoryIfNotExist();
                stream.SaveToFile(targetPath);
                stream.Dispose();
                Data.Add(new IdNameStatus(index, targetPath.GetFileName(), "等待执行"));
            });
        }

        [ObservableProperty]
        List<IdNameStatus> _data = new();

        [ObservableProperty]
        string _log;

        [RelayCommand]
        public async Task Execute()
        {
            await Task.Yield();
            Log = string.Empty;
            MainWindow.Instance.ViewModel.Running = true;

            var flag = true;
            foreach (var x in Data)
            {
                x.SetStaus("执行中");
                WriteLog($"执行:{x.Name}");

                try
                {
                    await Run(x);
                }
                catch (Exception ex)
                {
                    x.SetStaus("执行失败");
                    MessageBox.Show(ex.Message);
                    WriteLog($"执行失败:{ex.Message}");
                    flag = false;
                }
            }

            MainWindow.Instance.ViewModel.Running = false;
            if (flag)
            {
                MainWindow.Instance.SetPage<InstallPage>();
            }
        }

        async Task Run(IdNameStatus x)
        {
            await Task.Run(() =>
            {
                var path = AppDomain.CurrentDomain.BaseDirectory.CombinePath(x.Name);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(path)
                    {
                        WorkingDirectory = new FileInfo(path).DirectoryName,
                    }
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0) throw new Exception($"{x.Name}执行失败");
                x.SetStaus("执行成功");
                WriteLog($"执行成功:{x.Name}");
            });
        }

        void WriteLog(string text)
        {
            Log += $"{text}\r\n";
        }
    }

    public partial class IdNameStatus : ObservableObject
    {
        [ObservableProperty]
        int _id;

        [ObservableProperty]
        string _name;

        [ObservableProperty]
        string _status;

        public IdNameStatus(int id, string name, string status)
        {
            Id = id;
            Name = name;
            Status = status;
        }

        public void SetStaus(string status)
        {
            Status = status;
        }
    }
}
