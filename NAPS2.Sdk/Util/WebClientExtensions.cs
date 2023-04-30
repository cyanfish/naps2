using System.Net;
using System.Threading;

namespace NAPS2.Util;

internal static class WebClientExtensions
{
    public static Task<string> DownloadStringTaskAsync(this WebClient client, string address, CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<string>();
        CancellationTokenRegistration reg = default;

        void Completed(object sender, DownloadStringCompletedEventArgs e)
        {
            client.DownloadStringCompleted -= Completed;
            reg.Dispose();
            if (e.Cancelled)
            {
                tcs.TrySetCanceled();
            }
            else if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult(e.Result);
            }
        }

        client.DownloadStringCompleted += Completed;
        client.DownloadStringAsync(new Uri(address));
        reg = cancelToken.Register(client.CancelAsync);

        return tcs.Task;
    }

    public static Task<string> UploadStringTaskAsync(this WebClient client, string address, string method, string data, CancellationToken cancelToken = default)
    {
        var tcs = new TaskCompletionSource<string>();
        CancellationTokenRegistration reg = default;

        void Completed(object sender, UploadStringCompletedEventArgs e)
        {
            client.UploadStringCompleted -= Completed;
            reg.Dispose();
            if (e.Cancelled)
            {
                tcs.TrySetCanceled();
            }
            else if (e.Error != null)
            {
                tcs.TrySetException(e.Error);
            }
            else
            {
                tcs.TrySetResult(e.Result);
            }
        }

        client.UploadStringCompleted += Completed;
        client.UploadStringAsync(new Uri(address), method, data);
        reg = cancelToken.Register(client.CancelAsync);

        return tcs.Task;
    }

    // TODO: Use these extensions?
    public static void AddDownloadProgressHandler(this WebClient client, ProgressHandler progress)
    {
        client.DownloadProgressChanged += (sender, args) => progress.Report((int)args.BytesReceived, (int)args.TotalBytesToReceive);
    }

    public static void AddUploadProgressHandler(this WebClient client, ProgressHandler progress)
    {
        client.UploadProgressChanged += (sender, args) => progress.Report((int)args.BytesSent, (int)args.TotalBytesToSend);
    }
}