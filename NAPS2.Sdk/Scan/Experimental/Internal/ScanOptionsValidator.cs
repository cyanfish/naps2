using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Serialization;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental.Internal
{
    public class ScanOptionsValidator
    {
        public ScanOptions ValidateAll(ScanOptions options)
        {
            // Easy deep copy. Ideally we'd do this in a more efficient way.
            options = options.ToXml().FromXml<ScanOptions>();

            options.Driver = ValidateDriver(options);
            if (options.Driver == Driver.Sane)
            {
                options.UseNativeUI = false;
            }

            if (options.PageSize == null)
            {
                options.PageSize = PageSize.Letter;
            }
            if (options.Dpi == 0)
            {
                options.Dpi = 100;
            }
            if (options.Dpi < 0)
            {
                throw new ArgumentException("Invalid value for ScanOptions.Dpi.");
            }
            if (options.Brightness != options.Brightness.Clamp(-1000, 1000))
            {
                throw new ArgumentException("Invalid value for ScanOptions.Brightness.");
            }
            if (options.Contrast != options.Contrast.Clamp(-1000, 1000))
            {
                throw new ArgumentException("Invalid value for ScanOptions.Contrast.");
            }
            if (options.ScaleRatio == 0)
            {
                options.ScaleRatio = 1;
            }
            if (options.ScaleRatio < 0)
            {
                throw new ArgumentException("Invalid value for ScanOptions.ScaleRatio.");
            }

            // TODO: Validate DoOcr based on OcrParams

            return options;
        }

        public Driver ValidateDriver(ScanOptions options)
        {
            if (options.Driver == Driver.Default)
            {
                return GetSystemDefaultDriver();
            }
            // TODO: Throw NotSupportedException if the platform doesn't match the driver
            return options.Driver;
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
