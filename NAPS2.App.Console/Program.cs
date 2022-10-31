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
        // Use reflection to avoid antivirus false positives (yes, really)
        return (int) typeof(WindowsConsoleEntryPoint).GetMethod("Run")!.Invoke(null, new object[] { args })!;
    }
}