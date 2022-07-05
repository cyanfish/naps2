namespace NAPS2.Tools;

public static class Cli
{
    public static void Run(string command, string args)
    {
        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = command,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = Paths.SolutionRoot
        });
        if (proc == null)
        {
            throw new Exception($"Could not start {command}");
        }
        // TODO: Maybe we forward Console.CancelKeyPress
        while (!proc.WaitForExit(100))
        {
            PrintAll(proc.StandardOutput);
            PrintAll(proc.StandardError);
        }
        PrintAll(proc.StandardOutput);
        PrintAll(proc.StandardError);
        if (proc.ExitCode != 0)
        {
            throw new Exception($"Command failed: {command} {args}");
        }
    }

    private static void PrintAll(StreamReader stream)
    {
        string? line;
        while ((line = stream.ReadLine()) != null)
        {
            Console.WriteLine(line);
        }
    }
}