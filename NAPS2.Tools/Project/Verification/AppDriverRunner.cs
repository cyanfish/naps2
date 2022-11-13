using System.Threading;

namespace NAPS2.Tools.Project.Verification;

public class AppDriverRunner : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    public static AppDriverRunner Start()
    {
        return new AppDriverRunner();
    }

    private AppDriverRunner()
    {
        var path = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
        new Thread(() =>
        {
            Cli.Run(path, "", cancel: _cts.Token);
            // TODO: Wait for successful starting and handle errors (e.g. if the dev doesn't have developer mode on)
        }).Start();
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();
        }
        catch (Exception)
        {
            // Just trying to clean up
        }
    }
}