using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.Scan.Stub
{
    public class StubScanDriverFactory : IScanDriverFactory
    {
        private readonly IScannedImageFactory scannedImageFactory;

        public StubScanDriverFactory(IScannedImageFactory scannedImageFactory)
        {
            this.scannedImageFactory = scannedImageFactory;
        }

        public IScanDriver Create(string driverName)
        {
            return new StubScanDriver(driverName, scannedImageFactory);
        }
    }
}
