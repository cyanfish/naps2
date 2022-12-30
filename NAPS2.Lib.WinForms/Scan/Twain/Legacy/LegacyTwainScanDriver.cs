using System.Threading;
using NAPS2.Scan.Internal;

namespace NAPS2.Scan.Twain.Legacy;

internal class LegacyTwainScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public LegacyTwainScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        return Task.Run(() => Invoker.Current.InvokeGet(() => TwainApi.GetDeviceList(options)));
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        return Task.Run(() => Invoker.Current.Invoke(() => TwainApi.Scan(_scanningContext, options, callback)));
    }
}