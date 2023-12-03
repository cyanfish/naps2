using EmbedIO;

namespace NAPS2.Escl.Server;

internal static class WebServerExtensions
{
    public static async Task StartAsync(this WebServer server, CancellationToken cancelToken = default)
    {
        var startedTcs = new TaskCompletionSource<bool>();
        server.StateChanged += (_, args) =>
        {
            if (args.NewState == WebServerState.Listening)
            {
                startedTcs.TrySetResult(true);
            }
        };
        _ = server.RunAsync(cancelToken).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                startedTcs.TrySetException(t.Exception!);
            }
            else
            {
                startedTcs.TrySetCanceled();
            }
        });
        await startedTcs.Task;
    }
}