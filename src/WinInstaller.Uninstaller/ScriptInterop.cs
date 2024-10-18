using System.IO;
using WinInstaller.Uninstaller.Engine;

namespace WinInstaller.Uninstaller;

public class ScriptInterop : ScriptInteropBase
{
    public string GetInstallLocation() => App.CurrentInstance.Config.InstallLocation;

    public void UnInstall()
    {
        App.CurrentInstance.Running = true;
        App.CurrentInstance.WebBrowser.InvokeScript("setRunningState", true);
        //1.删除文件
        DeleteFolder(App.CurrentInstance.Config.InstallLocation);

        //2.删除注册表
        App.CurrentInstance.WebBrowser.InvokeScript("setLog", $"删除注册表");
        App.CurrentInstance.Config.Delete();

        //3.删除快捷方式
        var name = $"{App.CurrentInstance.Config.DisplayName}.lnk";
        var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), name);
        var userStartPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), name);
        App.CurrentInstance.WebBrowser.InvokeScript("setLog", $"删除快捷方式");
        if (File.Exists(desktopPath)) File.Delete(desktopPath);
        if (File.Exists(userStartPath)) File.Delete(userStartPath);
        App.CurrentInstance.WebBrowser.InvokeScript("setRunningState", true);
        App.CurrentInstance.Running = false;
    }

    void DeleteFolder(string path)
    {
        if (!Directory.Exists(path)) return;
        Directory.GetDirectories(path).ToList().ForEach(x => DeleteFolder(x));
        Directory.GetFiles(path).ToList().ForEach(x => DeleteFile(x));
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            App.CurrentInstance.WebBrowser.InvokeScript("setLog", $"删除文件夹:{path}");
        }
    }

    void DeleteFile(string path)
    {
        if (File.Exists(path)) File.Delete(path);
        App.CurrentInstance.WebBrowser.InvokeScript("setLog", $"删除文件:{path}");
    }

    public void Close()
    {
        App.CurrentInstance.MainWindow.Close();
    }
}
