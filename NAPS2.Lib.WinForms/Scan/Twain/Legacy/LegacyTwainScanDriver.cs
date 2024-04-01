using System.Threading;
using NAPS2.Scan.Internal;

namespace NAPS2.Scan.Twain.Legacy;

internal class LegacyTwainScanDriver : IScanDriver
{
    public Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        Check32Bit();
        return Task.Run(() => Invoker.Current.Invoke(() =>
        {
            foreach (var device in TwainApi.GetDeviceList(options))
            {
                callback(device);
            }
        }));
    }

    public Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        Check32Bit();
        return Task.Run(() => Invoker.Current.Invoke(() => TwainApi.Scan(options, callback)));
    }

    private static void Check32Bit()
    {
        if (Environment.Is64BitProcess)
        {
            throw new InvalidOperationException(
                "Can't run TWAIN with TwainAdapter.Legacy from a 64-bit process. You can set up a worker process with ScanningContext.SetUpWin32Worker().");
        }
    }
}