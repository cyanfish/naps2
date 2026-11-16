using System.ComponentModel;
using NAPS2.Remoting.Worker;

namespace NAPS2.Console;

/// <summary>
/// NAPS2.exe has all the actual NAPS2 code, but since Windows differentiates between GUI and CLI executables,
/// users are expected to use the console through NAPS2.Console.exe, which acts as a lightweight intermediary.
/// </summary>
static class Program
{
    static int Main()
    {
        var appPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NAPS2.exe");
        Process process;
        try
        {
            process = Process.Start(new ProcessStartInfo(appPath, $"console {GetRawArgs()}")
            {
                UseShellExecute = false
            })!;
        }
        catch (Win32Exception ex)
        {
            System.Console.Error.WriteLine($"Could not start {appPath}: {ex.Message}");
            return 1;
        }
        TieToCurrentProcess(process);

        // NAPS2.exe is attached to the same console and handles Ctrl+C itself, so we just keep waiting for it
        System.Console.CancelKeyPress += (_, e) => e.Cancel = true;

        using (process)
        {
            process.WaitForExit();
            return process.ExitCode;
        }
    }

    // Never disposed; Windows closes the handle when this process exits
    private static Job? _job;

    // If NAPS2.Console.exe is killed (e.g. by a script timeout), NAPS2.exe should be killed too
    private static void TieToCurrentProcess(Process process)
    {
        try
        {
            _job = new Job(allowBreakaway: true);
            _job.AddProcess(process.Handle);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    // Gets the original command line without the exe path, so arguments are forwarded exactly as given
    private static string GetRawArgs()
    {
        var commandLine = Environment.CommandLine;
        // The exe path is either quoted or ends at the first whitespace (with no escaping)
        int end = commandLine.StartsWith("\"")
            ? commandLine.IndexOf('"', 1)
            : commandLine.IndexOfAny([' ', '\t']);
        return end == -1 ? "" : commandLine.Substring(end + 1).TrimStart();
    }
}
