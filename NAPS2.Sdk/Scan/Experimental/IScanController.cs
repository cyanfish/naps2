using System.Collections.Generic;
using System.Threading;
using NAPS2.Images;

namespace NAPS2.Scan.Experimental
{
    public interface IScanController
    {
        List<ScanDevice> GetDeviceList(ScanOptions options);

        ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default);
    }
}