using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Proxy implementation of ITwainController that interacts with a Twain session in a worker process.
/// </summary>
internal class RemoteTwainController : ITwainController
{
    private readonly ScanningContext _scanningContext;

    public RemoteTwainController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        using var workerContext = CreateWorker(options);
        return await workerContext.Service.TwainGetDeviceList(options);
    }

    public async Task<ScanCaps> GetCaps(ScanOptions options)
    {
        using var workerContext = CreateWorker(options);
        return await workerContext.Service.TwainGetCaps(options);
    }

    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        using var workerContext = CreateWorker(options);
        await workerContext.Service.TwainScan(options, cancelToken, twainEvents);
        if (cancelToken.IsCancellationRequested)
        {
            // We need to report cancellation so that TwainImageProcessor doesn't return a partial image
            _scanningContext.Logger.LogDebug("NAPS2.TW - Sending cancel event");
            twainEvents.TransferCanceled(new TwainTransferCanceled());
        }
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