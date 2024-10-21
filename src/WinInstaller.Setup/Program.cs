using WinInstaller.Setup.Engine;

namespace WinInstaller.Setup;

public class Program
{
    [STAThread]
    public static void Main() => App.CurrentInstance.Run();
}