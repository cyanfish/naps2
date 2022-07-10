namespace NAPS2.Tools.Project.Verification;

public class AppDriverRunner : IDisposable
{
    private readonly Process _process;

    public static AppDriverRunner Start(bool verbose)
    {
        return new AppDriverRunner(verbose);
    }

    private AppDriverRunner(bool verbose)
    {
        var path = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
        // TODO: Wait for successful starting and handle errors (e.g. if the dev doesn't have developer mode on)
        _process = Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = false,
            // TODO: Fix (use Cli.Run from a thread? Or similar)
            // RedirectStandardOutput = !verbose
        }) ?? throw new Exception($"Could not start WinAppDriver: {path}");
    }

    public void Dispose()
    {
        try
        {
            _process.Kill();
        }
        catch (Exception)
        {
            // Just trying to clean up
        }
    }
}