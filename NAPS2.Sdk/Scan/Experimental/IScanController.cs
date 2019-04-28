using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
    public interface IScanController
    {
        // Based on the options and environment, the scan controller decides which implementation of IScanAdapter to use.
        // It also uses an instance of IImageFinalizer on images before returning them.

        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScanDevice PromptForDevice(ScanOptions options);

        ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default);
    }
}