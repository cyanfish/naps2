using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;

namespace NAPS2.Scan;

public interface IScanController
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

    ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default);
}