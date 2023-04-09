using NAPS2.Modules;

namespace NAPS2.EntryPoints;

public static class MacWorkerEntryPoint
{
    public static int Run(string[] args)
    {
        var app = NSApplication.SharedApplication;
        return WorkerEntryPoint.Run(args, new MacImagesModule(), () => app.Run(), () => app.Terminate(null));
    }
}
