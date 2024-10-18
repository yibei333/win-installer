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
        builder.AppendLine("<head>");
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
            var builder = new StringBuilder();
            if (extension == ".js")
            {
                builder.AppendLine("<script>");
                builder.AppendLine(content);
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
#if DEBUG
            MessageBox.Show(ex.Message);
#endif
            Debug.WriteLine(ex.StackTrace);
        }
        return "错误";
    }
}
