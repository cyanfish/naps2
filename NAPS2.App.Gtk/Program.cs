using NAPS2.EntryPoints;

namespace NAPS2;

static class Program
{
    /// <summary>
    /// The NAPS2 main method.
    /// </summary>
    static int Main(string[] args)
    {
        JitProfile.Start(args);
        return GtkEntryPoint.Run(args);
    }
}