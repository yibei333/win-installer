using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpDevLib;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using WinInstaller.Extensions;

namespace WinInstaller.Pages
{
    /// <summary>
    /// Interaction logic for LicensePage.xaml
    /// </summary>
    public partial class CompletePage : UserControl
    {
        public CompeleteViewModel ViewModel { get; set; } = new CompeleteViewModel();

        public CompletePage()
        {
            InitializeComponent();
        }
    }

    public partial class CompeleteViewModel : ObservableObject
    {
        [ObservableProperty]
        bool _run = true;

        [RelayCommand]
        public async Task Execute()
        {
            await Task.Yield();

            if (Run)
            {
                var exe = RegistryExtension.Get().CombinePath(App.EntryPoint);
                if (File.Exists(exe))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo(exe)
                        {
                            WorkingDirectory = new FileInfo(exe).DirectoryName,
                        }
                    };
                    process.Start();
                }
            }
            Application.Current.Shutdown();
            await Task.CompletedTask;
        }
    }
}
