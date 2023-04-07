using System.Threading;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Proxy implementation of ITwainSessionController that interacts with a Twain session in a worker process.
/// </summary>
public class RemoteTwainSessionController : ITwainSessionController
{
    private readonly ScanningContext _scanningContext;

    public RemoteTwainSessionController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        using var workerContext = CreateWorker(options);
        return await workerContext.Service.TwainGetDeviceList(options);
    }

    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        using var workerContext = CreateWorker(options);
        await workerContext.Service.TwainScan(options, cancelToken, twainEvents);
    }

    private WorkerContext CreateWorker(ScanOptions options)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            // Shouldn't hit this case
            throw new InvalidOperationException();
        }
        return _scanningContext.CreateWorker(
            options.TwainOptions.Dsm == TwainDsm.NewX64
                ? WorkerType.Native
                : WorkerType.WinX86)!;
    }
}