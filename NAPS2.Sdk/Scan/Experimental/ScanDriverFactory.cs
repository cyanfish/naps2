using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Scan.Experimental
{
    public class ScanDriverFactory : IScanDriverFactory
    {
        public IScanDriver Create(ScanOptions options)
        {
            var driver = options.Driver;
            if (driver == Driver.Default)
            {
                driver = GetSystemDefaultDriver();
            }
            switch (driver)
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

        private Driver GetSystemDefaultDriver()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    return Driver.Wia;
                case PlatformID.Unix:
                    return Driver.Sane;
                case PlatformID.MacOSX:
                    return Driver.Twain;
                default:
                    throw new InvalidOperationException("Unsupported operating system.");
            }
        }
    }
}