using System.Threading;

namespace NAPS2.Scan.Internal;

internal interface IScanDriver
{
    Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback);

    Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback);
}