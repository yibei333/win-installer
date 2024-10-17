using WinInstaller.Updater.Engine;

namespace WinInstaller.Updater;

public class Program
{
    [STAThread]
    public static void Main() => App.CurrentInstance.Run();
}