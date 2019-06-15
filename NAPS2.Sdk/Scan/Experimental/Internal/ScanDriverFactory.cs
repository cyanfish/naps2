using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Images.Storage;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class ScanDriverFactory : IScanDriverFactory
    {
        private readonly ImageContext imageContext;

        public ScanDriverFactory(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        public IScanDriver Create(ScanOptions options)
        {
            switch (options.Driver)
            {
                case Driver.Wia:
                    return new WiaScanDriver(imageContext);
                case Driver.Sane:
                    return new SaneScanDriver(imageContext);
                case Driver.Twain:
                    return options.TwainOptions.Adapter == TwainAdapter.Legacy
                        ? new LegacyTwainScanDriver()
                        : (IScanDriver)new TwainScanDriver(imageContext);
                default:
                    throw new InvalidOperationException("Unknown driver. Should never happen.");
            };
        }
    }
}