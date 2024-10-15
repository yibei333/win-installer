using System.IO;
using System.Reflection;
using System.Text;

namespace WinInstaller.Updater;

public static class EmbeddedResourceManager
{
    public static string Get(string name)
    {
        using var stream = Assembly.GetEntryAssembly().GetManifestResourceStream($"Web\\{name}") ?? throw new Exception($"找不到资源:{name}");
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var bytes = memoryStream.ToArray();
        return Encoding.UTF8.GetString(bytes);
    }
}
