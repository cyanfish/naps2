using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class ScanDriverFactory : IScanDriverFactory
    {
        public IScanDriver Create(ScanOptions options)
        {
            switch (options.Driver)
            {
                case Driver.Wia:
                    return new WiaScanDriver();
                case Driver.Sane:
                    return new SaneScanDriver();
                case Driver.Twain:
                    return options.TwainOptions.Adapter == TwainAdapter.Legacy
                        ? new LegacyTwainScanDriver()
                        : (IScanDriver)new TwainScanDriver();
                default:
                    throw new InvalidOperationException("Unknown driver. Should never happen.");
            };
        }
    }
}