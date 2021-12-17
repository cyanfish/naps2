using System.Threading;

namespace NAPS2.Scan;

public interface IScanController
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

    ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default);
}