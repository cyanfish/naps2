using System.Text;
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
        if (verbose)
        {
            Console.WriteLine($"{command} {args}");
        }
        var proc = Process.Start(startInfo);
        if (proc == null)
        {
            throw new Exception($"Could not start {command}");
        }
        cancel.Register(proc.Kill);
        // TODO: Maybe we forward Console.CancelKeyPress

        var savedOutput = new StringBuilder();
        void Save(object sender, DataReceivedEventArgs e)
        {
            savedOutput.AppendLine(e.Data);
        }

        proc.OutputDataReceived += verbose ? Print : Save;

        proc.ErrorDataReceived += Print;
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        while (!proc.WaitForExit(100))
        {
        }
        if (proc.ExitCode != 0 && !cancel.IsCancellationRequested)
        {
            if (!verbose)
            {
                Console.Write(savedOutput);
            }
            throw new Exception($"Command failed: {command} {args}");
        }
    }

    private static void Print(object sender, DataReceivedEventArgs e)
    {
        Console.WriteLine(e.Data);
    }
}