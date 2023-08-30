using System.Threading;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Represents scanning in a worker process on the same machine.
/// </summary>
internal class WorkerScanBridge : IScanBridge
{
    private readonly ScanningContext _scanningContext;
    private readonly WorkerType _workerType;

    public WorkerScanBridge(ScanningContext scanningContext, WorkerType workerType)
    {
        _scanningContext = scanningContext;
        _workerType = workerType;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException("ScanningContext must have a worker set up.");
        }
        using var ctx = _scanningContext.CreateWorker(_workerType)!;
        await ctx.Service.GetDevices(options, cancelToken, callback);
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<ProcessedImage, PostProcessingContext> callback)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException("ScanningContext must have a worker set up.");
        }
        using var ctx = _scanningContext.CreateWorker(_workerType)!;
        await ctx.Service.Scan(_scanningContext, options, cancelToken, scanEvents,
            (image, tempPath) => { callback(image, new PostProcessingContext { TempPath = tempPath }); });
    }
}