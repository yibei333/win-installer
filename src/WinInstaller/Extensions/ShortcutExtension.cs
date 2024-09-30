using SharpDevLib;
using System.IO;
using System.Runtime.InteropServices;

namespace WinInstaller.Extensions;

public static class ShortcutExtension
{
    public static string CreateShortcutToDesktop(string directory)
    {
        string app = directory.CombinePath(App.EntryPoint);
        string targetPath = directory.CombinePath($"{App.Name}.lnk");
        Create(targetPath, app, null, App.Name, "Ctrl+Shift+N");
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
        [TypeLibFunc((short)0x40), DispId(0x7d0)]
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
