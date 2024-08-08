#if MAC
using System.Threading;
using ImageCaptureCore;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Apple;

internal class AppleScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public AppleScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        using var reader = new DeviceReader();
        reader.DeviceFound += (_, args) =>
        {
            var device = args.Device;
            if (device.Uuid != null && device.Name != null)
            {
                callback(new ScanDevice(Driver.Apple, device.Uuid, device.Name));
            }
        };
        reader.Start();
        await Task.Delay(2000, cancelToken);
    }

    public async Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken)
    {
        using var reader = new DeviceReader();
        // Note we don't want to dispose the device, as the ICDeviceBrowser manages its lifetime.
        var device = await GetDevice(reader, options.Device!);
        using var oper =
            new DeviceOperator(_scanningContext, device, reader, options, cancelToken, null!, null!);
        return await oper.GetCaps();
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        using var reader = new DeviceReader();
        // Note we don't want to dispose the device, as the ICDeviceBrowser manages its lifetime.
        var device = await GetDevice(reader, options.Device!);
        using var oper =
            new DeviceOperator(_scanningContext, device, reader, options, cancelToken, scanEvents, callback);
        await oper.Scan();
    }

    private async Task<ICScannerDevice> GetDevice(DeviceReader reader, ScanDevice scanDevice)
    {
        var tcs = new TaskCompletionSource<ICScannerDevice>();
        reader.DeviceFound += (_, args) =>
        {
            if (args.Device.Uuid == scanDevice.ID)
            {
                tcs.TrySetResult(args.Device);
            }
        };
        reader.Start();
        Task.Delay(2000).ContinueWith(_ => tcs.TrySetCanceled()).AssertNoAwait();
        try
        {
            return await tcs.Task;
        }
        catch (TaskCanceledException)
        {
            throw new DeviceOfflineException();
        }
    }
}
#endif