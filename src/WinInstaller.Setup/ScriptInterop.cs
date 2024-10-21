using System.Diagnostics;
using System.IO;
using System.Windows;
using WinInstaller.Setup.Engine;

namespace WinInstaller.Setup;

public class ScriptInterop : ScriptInteropBase
{
    string _license = @"1.该软件为免费开源软件,可以用于任何场景.
2.请使用者注意备份数据,由该软件造成的数据丢失和任何问题,开发中不承担任何责任.
";
    static CancellationTokenSource cancle;

    public string GetIcon() => ResourceExtension.GetBase64Icon();

    public string GetCurrentVersion() => App.CurrentInstance.Config.DisplayVersion;

    public async void CheckUpdate()
    {
        try
        {
            App.CurrentInstance.Running = true;
            var version = await HttpHelper.Get(App.CurrentInstance.Config.VersionCheckUrl);
            App.CurrentInstance.WebBrowser.InvokeScript("setLastVersion", version);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        App.CurrentInstance.WebBrowser.InvokeScript("setRunningState", false);
        App.CurrentInstance.Running = false;
    }

    public async void DownloadAndUpdate()
    {
        try
        {
            App.CurrentInstance.Running = true;
            var directory = Path.Combine(App.CurrentInstance.Config.InstallLocation, "packages");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, $"{App.CurrentInstance.Config.DisplayName}.exe");

            cancle = new CancellationTokenSource();
            await HttpHelper.Download(App.CurrentInstance.Config.PackageDownloadUrl, path, p =>
            {
                App.CurrentInstance.Dispatcher.Invoke(() =>
                {
                    App.CurrentInstance.WebBrowser.InvokeScript("setProgress", p.Total, p.Handled, p.Progress, p.Speed);
                });
            }, cancle.Token);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo(path)
                {
                    WorkingDirectory = directory
                }
            };
            process.Start();
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        App.CurrentInstance.WebBrowser.InvokeScript("setRunningState", false);
        App.CurrentInstance.Running = false;
    }

    public void StopUpdate()
    {
        cancle?.Cancel();
    }
}
