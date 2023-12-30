using NAPS2.Remoting.Worker;

namespace NAPS2.Scan;

public static class ScanningContextExtensions
{
    /// <summary>
    /// If you have installed the NAPS2.Sdk.Worker.Win32 Nuget package, call this method to set up the worker on the
    /// ScanningContext. This will allow 32-bit TWAIN drivers to be used.
    /// </summary>
    public static void SetUpWin32Worker(this ScanningContext scanningContext)
    {
        var workerFactory = WorkerFactory.CreateDefault();
        if (!File.Exists(workerFactory.WinX86WorkerExePath))
        {
            throw new InvalidOperationException(
                "Could not find NAPS2.Worker.exe; have you installed the NAPS2.Sdk.Worker.Win32 Nuget package?");
        }
        workerFactory.Init(scanningContext);
        scanningContext.WorkerFactory = workerFactory;
    }
}