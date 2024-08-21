using System.Runtime;
using NAPS2.EntryPoints;

namespace NAPS2.Console;

static class Program
{
    /// <summary>
    /// The NAPS2.Console.exe main method.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        var profilesPath = Path.Combine(Paths.AppData, "jit");
        Directory.CreateDirectory(profilesPath);
        ProfileOptimization.SetProfileRoot(profilesPath);
        ProfileOptimization.StartProfile("naps2.console.jit");

        return WindowsConsoleEntryPoint.Run(args);
    }
}