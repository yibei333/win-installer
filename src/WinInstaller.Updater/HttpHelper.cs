using System.IO;
using System.Net;
using System.Net.Http;

namespace WinInstaller.Updater;

public static class HttpHelper
{
    public static async Task<string> Get(string url)
    {
        using var client = new HttpClient();
        return await client.GetStringAsync(url);
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
                    speed = speedNumber > 1024 ? $"{Math.Round(speedNumber / 1024, 2)}MB/S" : $"{speedNumber}KB/S";

                    time = now;
                    tempHanlded = 0;
                }
                var progressValue = Math.Round(handled * 100.0 / total, 2);
                progress?.Invoke(new ProgressModel { Total = total, Handled = handled, Speed = speed, Progress = progressValue });
            }
        });
    }
}
