using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images.Storage;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public class ScanDriverController : IScanDriverController
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) =>
            GetDriver(options).GetDeviceList(options);

        public ScanDevice PromptForDevice(ScanOptions options) =>
            GetDriver(options).PromptForDevice(options);

        public void Scan(ScanOptions options, ProgressHandler progress, CancellationToken cancelToken, Action<IImage> callback) =>
            GetDriver(options).Scan(options, progress, cancelToken, callback);

        private IScanDriver GetDriver(ScanOptions options)
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
