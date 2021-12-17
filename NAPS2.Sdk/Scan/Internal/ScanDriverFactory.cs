using System;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Internal;

internal class ScanDriverFactory : IScanDriverFactory
{
    private readonly ImageContext _imageContext;

    public ScanDriverFactory(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public IScanDriver Create(ScanOptions options)
    {
        switch (options.Driver)
        {
            case Driver.Wia:
                return new WiaScanDriver(_imageContext);
            case Driver.Sane:
                return new SaneScanDriver(_imageContext);
            case Driver.Twain:
                return options.TwainOptions.Adapter == TwainAdapter.Legacy
                    ? new LegacyTwainScanDriver()
                    : (IScanDriver)new TwainScanDriver(_imageContext);
            default:
                throw new InvalidOperationException("Unknown driver. Should never happen.");
        };
    }
}