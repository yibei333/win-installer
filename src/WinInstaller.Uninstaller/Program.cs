using WinInstaller.Uninstaller.Engine;

namespace WinInstaller.Uninstaller;

public class Program
{
    [STAThread]
    public static void Main() => App.CurrentInstance.Run();
}