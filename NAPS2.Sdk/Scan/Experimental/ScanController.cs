using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
    public class ScanController : IScanController
    {
        public List<ScanDevice> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

        public ScanDevice PromptForDevice(ScanOptions options) => throw new NotImplementedException();

        public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default) => throw new NotImplementedException();
    }
}
