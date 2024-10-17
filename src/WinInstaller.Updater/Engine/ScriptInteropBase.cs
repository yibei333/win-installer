using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace WinInstaller.Updater.Engine;

public abstract class ScriptInteropBase
{
    public string BuildHtml()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<html>");
#if DEBUG
        builder.AppendLine("<body>");
#else
        builder.AppendLine("<body oncontextmenu='return false'>");
#endif
        builder.AppendLine(LoadResource("lib.js"));
        builder.AppendLine(LoadResource("lib.css"));
        builder.AppendLine(LoadResource("main.css"));
        builder.AppendLine(LoadResource("main.js"));
        var html = LoadResource("main.html");
        html = html.Replace("<link rel=\"stylesheet\" href=\"lib.css\" />", "");
        html = html.Replace("<link rel=\"stylesheet\" href=\"main.css\" />", "");
        html = html.Replace("<link rel=\"stylesheet\" href=\"main-dev.css\" />", "");
        html = html.Replace("<script src=\"lib.js\"></script>", "");
        html = html.Replace("<script src=\"main.js\"></script>", "");
        html = html.Replace("<script src=\"main-dev.js\"></script>", "");
        builder.AppendLine(html);
        builder.AppendLine("</body>");
        builder.AppendLine("</html>");
        return builder.ToString();
    }

    string LoadResource(string name)
    {
        try
        {
            var content = name.GetResourceText();
            var extension = new FileInfo(name).Extension;
            if (extension == ".js")
            {
                return $"<script>{content}</script>";
            }
            else if (extension == ".css")
            {
                return $"<style>{content}</style>";
            }
            else
            {
                return content;
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            MessageBox.Show(ex.Message);
#endif
            Debug.WriteLine(ex.StackTrace);
        }
        return "nope";
    }

    public void SetRunningState(bool running)
    {
        App.CurrentInstance.Running = running;
    }

    public void Alert(string message) => MessageBox.Show(message);
}
