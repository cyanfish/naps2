using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Represents scanning across a network on a different machine.
/// </summary>
internal class NetworkScanBridge : IScanBridge
{
    private readonly ScanningContext _scanningContext;

    public NetworkScanBridge(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

    // TODO: On the network server, make sure to throttle progress events
    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ProcessedImage, PostProcessingContext> callback) => throw new NotImplementedException();
}