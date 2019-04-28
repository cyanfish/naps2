using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;
using NAPS2.Util;

namespace NAPS2.Scan.Experimental
{
    public interface IScanController
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScannedImageSource Scan(ScanOptions options, ProgressHandler progress = default, CancellationToken cancelToken = default);
    }
}