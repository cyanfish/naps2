using System.Threading;

namespace NAPS2.Scan.Internal;

internal interface IScanDriver
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

    Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback);
}