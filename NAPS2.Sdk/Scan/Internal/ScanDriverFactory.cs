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
#if MAC
            case Driver.Apple:
                return new Apple.AppleScanDriver(_scanningContext);
#else
            case Driver.Wia:
                return new Wia.WiaScanDriver(_scanningContext);
            case Driver.Twain:
                return options.TwainOptions.Adapter == TwainAdapter.Legacy
                    ? new Twain.LegacyTwainScanDriver()
                    : new Twain.TwainScanDriver(_scanningContext);
#endif
            case Driver.Sane:
                return new Sane.SaneScanDriver(_scanningContext);
            default:
                throw new NotSupportedException(
                    $"Unsupported driver: {options.Driver}. " +
                    "Make sure you're using the right framework target (e.g. net6-macos10.15 for the Apple driver).");
        }
    }
}