using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using WinInstaller.Setup.Engine;

namespace WinInstaller.Setup;

public static class Extension
{
    public static string FormatPath(this string path)
    {
        return path.Replace("\\", "/").Trim();
    }

    //public static string SelectPath()
    //{
    //    var dialog = new System.Windows.Forms.FolderBrowserDialog { Description = "选择安装位置" };
    //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //    {
    //        var path = dialog.SelectedPath.FormatPath().TrimEnd("/");
    //        if (!path.EndsWith(App.Name)) path = path.CombinePath(App.Name);
    //        Location = path;
    //    }
    //}

    //unzip

    public static bool StopRunningApp()
    {
        var name = new FileInfo(App.CurrentInstance.Config.EntryPoint).Name;
        var exePath = Path.Combine(App.CurrentInstance.Config.InstallLocation, App.CurrentInstance.Config.EntryPoint).FormatPath();
        var processes = Process.GetProcessesByName(name).Where(x => x.MainModule.FileName.FormatPath() == exePath).ToList();
        if (!processes.Any()) return true;

        if (MessageBox.Show("确定停止正在运行的程序?", "消息确认", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
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

    public static string CreateShortcut(string directory)
    {
        string app = Path.Combine(directory, App.CurrentInstance.Config.EntryPoint);
        string targetPath = Path.Combine(directory, $"{App.CurrentInstance.Config.DisplayName}.lnk");
        Create(targetPath, app, null, App.CurrentInstance.Config.DisplayName, "Ctrl+Shift+N");
        return targetPath;
    }

    [ComImport, TypeLibType(0x1040), Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
    interface IWshShortcut
    {
        [DispId(0)]
        string FullName { [return: MarshalAs(UnmanagedType.BStr)][DispId(0)] get; }
        [DispId(0x3e8)]
        string Arguments { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3e8)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3e8)] set; }
        [DispId(0x3e9)]
        string Description { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3e9)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3e9)] set; }
        [DispId(0x3ea)]
        string Hotkey { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3ea)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3ea)] set; }
        [DispId(0x3eb)]
        string IconLocation { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3eb)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3eb)] set; }
        [DispId(0x3ec)]
        string RelativePath { [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3ec)] set; }
        [DispId(0x3ed)]
        string TargetPath { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3ed)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3ed)] set; }
        [DispId(0x3ee)]
        int WindowStyle { [DispId(0x3ee)] get; [param: In][DispId(0x3ee)] set; }
        [DispId(0x3ef)]
        string WorkingDirectory { [return: MarshalAs(UnmanagedType.BStr)][DispId(0x3ef)] get; [param: In, MarshalAs(UnmanagedType.BStr)][DispId(0x3ef)] set; }
        [TypeLibFunc(0x40), DispId(0x7d0)]
        void Load([In, MarshalAs(UnmanagedType.BStr)] string PathLink);
        [DispId(0x7d1)]
        void Save();
    }

    static void Create(string fileName, string targetPath, string arguments, string description, string hotkey)
    {
        var type = Type.GetTypeFromProgID("WScript.Shell");
        var shell = Activator.CreateInstance(type);
        IWshShortcut shortcut = (IWshShortcut)type.InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { fileName });
        shortcut.Description = description;
        shortcut.Hotkey = hotkey;
        shortcut.TargetPath = targetPath;
        shortcut.WorkingDirectory = new FileInfo(targetPath).DirectoryName;
        shortcut.Arguments = arguments;
        shortcut.Save();
    }
}
