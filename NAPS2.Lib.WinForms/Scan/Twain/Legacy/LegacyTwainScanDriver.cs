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

    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        return Task.Run(() => Invoker.Current.Invoke(() =>
        {
            foreach (var device in TwainApi.GetDeviceList(options))
            {
                callback(device);
            }
        }));
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<IMemoryImage> callback)
    {
        return Task.Run(() => Invoker.Current.Invoke(() => TwainApi.Scan(_scanningContext, options, callback)));
    }
}