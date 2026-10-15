using NAPS2.Images.Gdi;
using NAPS2.Remoting.Worker;
using NAPS2.Scan;

namespace NAPS2.Sdk.Worker;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
public class Program
{
    [STAThread]
    public static void Main()
    {
        var scanningContext = new ScanningContext(new GdiImageContext());
        // We can't use "async Main" with STAThread
        WorkerServer.Run(scanningContext).GetAwaiter().GetResult();
    }
}