namespace NAPS2.Scan.Internal;

internal interface IScanBridgeFactory
{
    IScanBridge Create(ScanOptions options);
}