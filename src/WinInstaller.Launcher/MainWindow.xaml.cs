using System.Diagnostics;
using System.IO;
using System.Windows;

namespace WinInstaller.Launcher
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Run();
        }

        void Run()
        {
            try
            {
                var config = Config.Get();
                if (string.IsNullOrWhiteSpace(config.EntryPoint)) throw new Exception("找不到应用程序入口");
                var entryPoint = Path.Combine(config.InstallLocation, config.DisplayVersion, config.EntryPoint);
                if (!File.Exists(entryPoint)) throw new Exception($"找不到文件:'{entryPoint}'");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(entryPoint)
                    {
                        WorkingDirectory = new FileInfo(entryPoint).DirectoryName
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }
    }
}
