namespace NAPS2.Scan.Internal;

internal class ScanBridgeFactory : IScanBridgeFactory
{
    private readonly InProcScanBridge _inProcScanBridge;
    private readonly NetworkScanBridge _networkScanBridge;

    public ScanBridgeFactory(ScanningContext scanningContext)
        : this(new InProcScanBridge(scanningContext), new NetworkScanBridge(scanningContext))
    {
    }

    public ScanBridgeFactory(InProcScanBridge inProcScanBridge, NetworkScanBridge networkScanBridge)
    {
        _inProcScanBridge = inProcScanBridge;
        _networkScanBridge = networkScanBridge;
    }

    public IScanBridge Create(ScanOptions options)
    {
        if (!string.IsNullOrEmpty(options.NetworkOptions.Ip))
        {
            // The physical scanner is connected to a different computer, so we connect to a NAPS2 server process over the network
            return _networkScanBridge;
        }
        return _inProcScanBridge;
    }
}
