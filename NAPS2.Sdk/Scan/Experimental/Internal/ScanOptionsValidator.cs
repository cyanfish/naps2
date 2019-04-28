using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Serialization;

namespace NAPS2.Scan.Experimental.Internal
{
    internal class ScanOptionsValidator
    {
        public ScanOptions Validate(ScanOptions options)
        {
            // Easy deep copy. Ideally we'd do this in a more efficient way.
            options = options.ToXml().FromXml<ScanOptions>();

            if (options.Driver == Driver.Default)
            {
                options.Driver = GetSystemDefaultDriver();
            }

            return options;
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
