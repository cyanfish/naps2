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

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        using var reader = new DeviceReader();
        reader.Start();
        await Task.Delay(2000);
        return reader.Devices.Select(x => new ScanDevice(x.Uuid, x.Name)).ToList();
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        using var reader = new DeviceReader();
        using var device = await GetDevice(reader, options.Device!);
        using var oper =
            new DeviceOperator(_scanningContext, device, options, cancelToken, scanEvents, callback);
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
            throw new DeviceException(SdkResources.DeviceOffline);
        }
    }
}
#endif