using System.IO;
using System.Net;
using System.Text;

namespace WinInstaller.Updater.Engine;

public static class HttpHelper
{
    public static async Task<string> Get(string url)
    {
        var request = WebRequest.Create(url);
        var response = await request.GetResponseAsync();
        using var stream = response.GetResponseStream();
        using var targetStream = new MemoryStream();
        stream.CopyTo(targetStream);
        var text = Encoding.UTF8.GetString(targetStream.ToArray());
        return text;
    }

    public static async Task Download(string url, string savePath, Action<ProgressModel> progress, CancellationToken cancellationToken)
    {
        await Task.Run(async () =>
        {
            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var total = response.ContentLength;
            using var stream = response.GetResponseStream();
            using var saveStream = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            var buffer = new byte[1024 * 1024];
            long handled = 0;
            var length = 0;
            var time = DateTime.Now;
            string speed = "0";
            var tempHanlded = 0;
            while ((length = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (cancellationToken.IsCancellationRequested) throw new Exception("任务取消");
                handled += length;
                saveStream.Write(buffer, 0, length);

                var now = DateTime.Now;
                var millSeconds = (now - time).TotalMilliseconds;
                tempHanlded += length;
                if (millSeconds > 1000)
                {
                    var speedNumber = Math.Round(tempHanlded * 1000 / (millSeconds * 1024), 2);
                    speed = speedNumber > 1024 ? $"{Math.Round(speedNumber / 1024, 2)}MB/s" : $"{speedNumber}KB/s";

                    time = now;
                    tempHanlded = 0;
                }
                var progressValue = Math.Round(handled * 100.0 / total, 2);
                progress?.Invoke(new ProgressModel(total, handled, progressValue, speed));
            }
        });
    }
}
