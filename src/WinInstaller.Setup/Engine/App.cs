using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace WinInstaller.Setup.Engine;

public class App : Application
{
    static readonly App _instance = new();
    public static App CurrentInstance => _instance;

    private App()
    {
        Assembly = Assembly.GetExecutingAssembly();
        Config = Config.Create();
        SetWindow();
        SetBrowser();
        Current.MainWindow.Show();
    }

    public Assembly Assembly { get; }

    public Config Config { get; }

    public WebBrowser WebBrowser { get; private set; }

    public bool Running { get; set; }

    void SetWindow()
    {
        var window = new Window
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            Title = $"{Config.DisplayName}-{Config.DisplayVersion}-安装程序",
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Closing += (s, e) =>
        {
            if (Running)
            {
                MessageBox.Show("请等待任务执行完成");
                e.Cancel = true;
                return;
            }
        };
        Current.MainWindow = window;
    }

    void SetBrowser()
    {
        WebBrowser = new WebBrowser
        {
            Width = 500,
            Height = 250
        };

        //js interop
        var type = Assembly.GetTypes().Where(x => x.BaseType.Equals(typeof(ScriptInteropBase))).FirstOrDefault();
        if (type is null)
        {
            MessageBox.Show("unable to find ScriptInterop");
            Current.Shutdown();
        }
        var scriptInterop = Activator.CreateInstance(type) as ScriptInteropBase;
        WebBrowser.ObjectForScripting = scriptInterop;

        //navigate
        WebBrowser.NavigateToString(scriptInterop.BuildHtml());

        Current.MainWindow.Content = WebBrowser;
    }
}