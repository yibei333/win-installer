using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace WinInstaller.Uninstaller.Engine;

public static class ResourceExtension
{
    public static string GetResourceText(this string name)
    {
        using var stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"Web\\{name}") ?? throw new Exception($"找不到资源:{name}");
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        return Encoding.UTF8.GetString(bytes);
    }

    public static string GetBase64Icon()
    {
        using var stream = Application.GetResourceStream(new Uri("pack://application:,,,/favicon.ico")).Stream;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        var image = Convert.ToBase64String(bytes);
        return $"data:image/x-icon;base64,{image}";
    }
}
