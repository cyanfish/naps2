using System.Runtime;
using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2.app main method.
    /// </summary>
    static void Main(string[] args)
    {
        var profilesPath = Path.Combine(Paths.AppData, "jit");
        Directory.CreateDirectory(profilesPath);
        ProfileOptimization.SetProfileRoot(profilesPath);
        ProfileOptimization.StartProfile("naps2.jit");

        // Use reflection to avoid antivirus false positives (yes, really)
        typeof(MacEntryPoint).GetMethod("Run").Invoke(null, new object[] { args });
    }
}