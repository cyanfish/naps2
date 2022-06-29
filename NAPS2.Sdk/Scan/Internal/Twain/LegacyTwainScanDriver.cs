using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

internal class LegacyTwainScanDriver : IScanDriver
{
    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options) => throw new NotImplementedException();

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        throw new NotImplementedException();
    }
}