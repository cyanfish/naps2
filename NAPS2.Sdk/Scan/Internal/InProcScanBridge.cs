using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Represents scanning in the local process.
/// </summary>
internal class InProcScanBridge : IScanBridge
{
    private readonly IRemoteScanController _remoteScanController;

    public InProcScanBridge(ScanningContext scanningContext)
        : this(new RemoteScanController(scanningContext))
    {
    }

    public InProcScanBridge(IRemoteScanController remoteScanController)
    {
        _remoteScanController = remoteScanController;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) =>
        _remoteScanController.GetDeviceList(options);

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<RenderableImage, PostProcessingContext> callback) =>
        _remoteScanController.Scan(options, cancelToken, scanEvents, callback);
}