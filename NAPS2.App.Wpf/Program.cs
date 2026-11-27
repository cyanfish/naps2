using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2.exe main method.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        JitProfile.Start(args, "naps2.wpf");
        return WpfEntryPoint.Run(args);
    }
}
