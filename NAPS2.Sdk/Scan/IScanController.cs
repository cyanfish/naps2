using System.Threading;

namespace NAPS2.Scan;

public interface IScanController
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

    AsyncSource<ProcessedImage> Scan(ScanOptions options, CancellationToken cancelToken = default);
}