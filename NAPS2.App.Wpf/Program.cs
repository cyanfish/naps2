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
        WpfEntryPoint.Run(args);
    }
}
