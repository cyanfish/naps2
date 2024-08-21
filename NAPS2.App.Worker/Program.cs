using System.Runtime;
using NAPS2.EntryPoints;

namespace NAPS2.Worker;

static class Program
{
    /// <summary>
    /// The NAPS2.Worker.exe main method.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        var profilesPath = Path.Combine(Paths.AppData, "jit");
        Directory.CreateDirectory(profilesPath);
        ProfileOptimization.SetProfileRoot(profilesPath);
        ProfileOptimization.StartProfile("naps2.worker.jit");

        WindowsWorkerEntryPoint.Run(args);
    }
}