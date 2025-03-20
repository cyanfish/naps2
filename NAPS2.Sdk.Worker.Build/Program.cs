using NAPS2.Images.Gdi;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;

namespace NAPS2.Sdk.Worker;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class Program
{
    public static async Task Main()
    {
        var scanningContext = new ScanningContext(new GdiImageContext());
        await WorkerServer.Run(scanningContext);
    }
}