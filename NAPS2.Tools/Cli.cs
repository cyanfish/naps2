using System.Threading;

namespace NAPS2.Tools;

public static class Cli
{
    public static void Run(string command, string args, bool verbose, Dictionary<string, string>? env = null, CancellationToken cancel = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = Paths.SolutionRoot
        };
        if (env != null)
        {
            foreach (var kvp in env)
            {
                startInfo.EnvironmentVariables[kvp.Key] = kvp.Value;
            }
        }
        var proc = Process.Start(startInfo);
        if (proc == null)
        {
            throw new Exception($"Could not start {command}");
        }
        cancel.Register(proc.Kill);
        // TODO: Maybe we forward Console.CancelKeyPress
        while (!proc.WaitForExit(100))
        {
            PrintAll(proc.StandardOutput, verbose);
            PrintAll(proc.StandardError, true);
        }
        PrintAll(proc.StandardOutput, verbose);
        PrintAll(proc.StandardError, true);
        if (proc.ExitCode != 0)
        {
            throw new Exception($"Command failed: {command} {args}");
        }
    }

    private static void PrintAll(StreamReader stream, bool forwardToStdout)
    {
        string? line;
        while ((line = stream.ReadLine()) != null)
        {
            if (forwardToStdout)
            {
                Console.WriteLine(line);
            }
        }
    }
}