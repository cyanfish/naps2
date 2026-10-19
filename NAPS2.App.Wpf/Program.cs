using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2.exe main method.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // TODO: Use the WPF entry point once it's implemented
        WinFormsEntryPoint.Run(args);
    }
}
