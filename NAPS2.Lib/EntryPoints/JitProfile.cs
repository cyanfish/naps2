using System.Runtime;

namespace NAPS2.EntryPoints;

/// <summary>
/// Starts a multi-core JIT profile, which records the methods compiled during startup so that later launches can
/// compile them ahead of time on background threads.
/// </summary>
public static class JitProfile
{
    public static void Start(string[] args, string prefix = "naps2")
    {
        var profilesPath = Path.Combine(Paths.AppData, "jit");
        Directory.CreateDirectory(profilesPath);
        ProfileOptimization.SetProfileRoot(profilesPath);
        var mode = args switch
        {
            ["cli" or "console", ..] => ".console",
            ["worker", ..] => ".worker",
            ["server", ..] => ".server",
            _ => ""
        };
        ProfileOptimization.StartProfile($"{prefix}{mode}.jit");
    }
}
