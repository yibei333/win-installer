using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WinInstaller.Updater;

[ComVisible(true)]
public class JavascriptManager
{
    public JavascriptManager(WebBrowser browser)
    {
        Browser = browser;
        Browser.NavigateToString(BuildHtml());
    }

    public WebBrowser Browser { get; }

    #region Init
    string BuildHtml()
    {
        var builder = new StringBuilder();
        builder.AppendLine("<html>");
#if DEBUG
        builder.AppendLine("<body>");
#else
        builder.AppendLine("<body oncontextmenu='return false'>");
#endif
        builder.AppendLine(LoadResource("main.css"));
        builder.AppendLine(LoadResource("main.js"));
        var html = LoadResource("main.html");
        html = html.Replace("<link rel=\"stylesheet\" href=\"main.css\" />", "");
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
            var content = EmbeddedResourceManager.Get(name);
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
        }
        return "nope";
    }
    #endregion

    public string SharpMethod(string text)
    {
        return $"sharp response:{text}";
    }
}
