using NAPS2.Scan.Exceptions;

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
#if NET6_0_OR_GREATER
                if (!OperatingSystem.IsWindows()) throw new NotSupportedException();
#endif
                return new Wia.WiaScanDriver(_scanningContext);
            case Driver.Twain:
#if NET6_0_OR_GREATER
                if (!OperatingSystem.IsWindows()) throw new NotSupportedException();
#endif
                return new Twain.TwainScanDriver(_scanningContext);
#endif
            case Driver.Sane:
                return new Sane.SaneScanDriver(_scanningContext);
            case Driver.Escl:
                return new Escl.EsclScanDriver(_scanningContext);
            default:
                throw new DriverNotSupportedException(
                    $"Unsupported driver: {options.Driver}. " +
                    "Make sure you're using the right framework target (e.g. net8-macos for the Apple driver).");
        }
    }
}