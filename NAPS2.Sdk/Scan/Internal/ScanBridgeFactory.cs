using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal;

internal class ScanBridgeFactory : IScanBridgeFactory
{
    private readonly ScanningContext _scanningContext;

    public ScanBridgeFactory(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public IScanBridge Create(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            // Worker processes generally aren't required, just preferred for stability.
            // Where applicable, the driver (i.e. Twain) will throw an error if we're running on the wrong arch.
            return new InProcScanBridge(_scanningContext);
        }
        if (options.Driver == Driver.Apple)
        {
            // Run ImageCaptureCore in a worker process for added stability
            return new WorkerScanBridge(_scanningContext, WorkerType.Native);
        }
        if (options.Driver == Driver.Sane)
        {
            // Run SANE in a worker process for added stability
            return new WorkerScanBridge(_scanningContext, WorkerType.Native);
        }
        return new InProcScanBridge(_scanningContext);
    }
}