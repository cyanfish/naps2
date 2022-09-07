using NAPS2.Scan.Internal.Sane;
using NAPS2.Scan.Internal.Twain;
using NAPS2.Scan.Internal.Wia;

namespace NAPS2.Scan.Internal;

internal class ScanDriverFactory : IScanDriverFactory
{
    private readonly ScanningContext _scanningContext;

    public ScanDriverFactory(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public IScanDriver Create(ScanOptions options)
    {
        switch (options.Driver)
        {
            case Driver.Wia:
                return new WiaScanDriver(_scanningContext);
            case Driver.Sane:
                return new SaneScanDriver(_scanningContext);
            case Driver.Twain:
                return options.TwainOptions.Adapter == TwainAdapter.Legacy
                    ? new LegacyTwainScanDriver()
                    : new TwainScanDriver(_scanningContext);
            default:
                throw new InvalidOperationException("Unknown driver. Should never happen.");
        };
    }
}