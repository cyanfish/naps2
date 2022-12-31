#if !MAC
using System.Runtime.InteropServices;
using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Implementation of IScanDriver for Twain. Delegates to RemoteTwainSessionController in most cases which runs Twain
/// in a 32-bit worker process as Twain drivers are generally 32-bit only. 
/// </summary>
internal class TwainScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public TwainScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        var controller = GetSessionController(options);
        return await controller.GetDeviceList(options);
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        var controller = GetSessionController(options);
        using var state = new TwainImageProcessor(_scanningContext, options, scanEvents, callback);
        await controller.StartScan(options, state, cancelToken);
    }

    private ITwainSessionController GetSessionController(ScanOptions options)
    {
        return new RemoteTwainSessionController(_scanningContext);
    }
}
#endif