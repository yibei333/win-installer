using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace WinInstaller.Uninstaller.Engine;

public abstract class ScriptInteropBase
{
    public string BuildHtml()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<html lang=\"en\">");
        builder.AppendLine("<head>");
        builder.AppendLine("<meta charset=\"UTF-8\">");
        builder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no\">");
        builder.AppendLine("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge,chrome=1\"/>");
        builder.AppendLine("</head>");
#if DEBUG
        builder.AppendLine("<body>");
#else
        builder.AppendLine("<body oncontextmenu='return false'>");
#endif
        builder.AppendLine(LoadResource("lib.js"));
        builder.AppendLine(LoadResource("lib.css"));
        builder.AppendLine(LoadResource("main.css"));
        builder.AppendLine(LoadResource("main-external.js"));
        builder.AppendLine(LoadResource("main.js"));
        var html = LoadResource("main.html");
        html = html.Replace("<link rel=\"stylesheet\" href=\"lib.css\" />", "");
        html = html.Replace("<link rel=\"stylesheet\" href=\"main.css\" />", "");
        html = html.Replace("<link rel=\"stylesheet\" href=\"main-dev.css\" />", "");
        html = html.Replace("<script src=\"lib.js\"></script>", "");
        html = html.Replace("<script src=\"main-external-dev.js\"></script>", "");
        html = html.Replace("<script src=\"main.js\"></script>", "");
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
            var builder = new StringBuilder();
            if (extension == ".js")
            {
                builder.AppendLine("<script>");
                builder.AppendLine("try{");
                builder.AppendLine(content);
                builder.AppendLine("}");
                builder.AppendLine("catch(e){");
                builder.AppendLine("window.external.HandleScriptError(e);");
                builder.AppendLine("}");
                builder.AppendLine("</script>");
                return builder.ToString();
            }
            else if (extension == ".css")
            {
                builder.AppendLine("<style>");
                builder.AppendLine(content);
                builder.AppendLine("</style>");
                return builder.ToString();
            }
            else
            {
                return content;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            Debug.WriteLine(ex.StackTrace);
            Environment.Exit(0);
            return null;
        }
    }

    public void HandleScriptError(string error)
    {
        MessageBox.Show(error);
        Environment.Exit(0);
    }
}
