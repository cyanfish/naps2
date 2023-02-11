using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Represents scanning in the local process.
/// </summary>
internal class InProcScanBridge : IScanBridge
{
    private readonly IRemoteScanController _remoteScanController;

    public InProcScanBridge(ScanningContext scanningContext)
        : this(scanningContext, new RemoteScanController(scanningContext))
    {
    }

    public InProcScanBridge(ScanningContext scanningContext, IRemoteScanController remoteScanController)
    {
        _remoteScanController = remoteScanController;
    }

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback) =>
        _remoteScanController.GetDevices(options, cancelToken, callback);

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ProcessedImage, PostProcessingContext> callback) =>
        _remoteScanController.Scan(options, cancelToken, scanEvents, callback);
}