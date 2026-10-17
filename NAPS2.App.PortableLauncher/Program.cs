using System.Reflection;
using System.Threading;

namespace NAPS2.Portable;

class Program
{
    static void Main(string[] args)
    {
        var portableExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (portableExeDir != null)
        {
            bool failedUpdate = false;
            bool isUpdate = args.Length == 3 && args[0] == "/Update";
            try
            {
                if (isUpdate)
                {
                    failedUpdate = true;
                    UpdatePortableApp(portableExeDir, args[1], args[2]);
                    failedUpdate = false;
                }
            }
            finally
            {
                var portableExePath = Path.Combine(portableExeDir, "App", "NAPS2.exe");
                if (failedUpdate)
                {
                    Process.Start(portableExePath, "/FailedUpdate");
                }
                else if (isUpdate)
                {
                    Process.Start(portableExePath);
                }
                else
                {
                    Process.Start(portableExePath, GetRawArgs());
                }
            }
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

    private static void UpdatePortableApp(string portableExeDir, string procId, string newAppFolderPath)
    {
        // Wait for the starting process and workers to finish so we don't try to mess with files in use
        try
        {
            var stopwatch = Stopwatch.StartNew();

            // Wait for the starting process to exit
            var mainProc = Process.GetProcessById(int.Parse(procId));
            mainProc.WaitForExit();

            // Assume any process named NAPS2 or NAPS2.Worker could be a worker, although this isn't necessarily true
            // if there are multiple NAPS2 installations.
            var processesToWaitOn =
                Process.GetProcessesByName("NAPS2")
                    .Concat(Process.GetProcessesByName("NAPS2.Worker"))
                    .ToList();

            // Wait at most 10 seconds for them to exit (which is WorkerEntryPoint.ParentCheckInterval)
            const int waitTimeout = 10_000;
            while (stopwatch.ElapsedMilliseconds < waitTimeout && processesToWaitOn.Any(x => !x.HasExited))
            {
                Thread.Sleep(100);
            }
        }
        catch (ArgumentException)
        {
        }

        // Safely replace the old App folder
        AtomicReplaceDirectory(newAppFolderPath, Path.Combine(portableExeDir, "App"));
    }

    private static void AtomicReplaceDirectory(string source, string dest)
    {
        string temp = dest + ".old";
        Directory.Move(dest, temp);
        try
        {
            Directory.Move(source, dest);
            Directory.Delete(temp, true);
        }
        catch (Exception)
        {
            Directory.Move(temp, dest);
            throw;
        }
    }
}