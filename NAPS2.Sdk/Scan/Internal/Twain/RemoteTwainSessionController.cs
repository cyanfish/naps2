using System.Threading;

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
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException();
        }
        using var workerContext = _scanningContext.WorkerFactory.Create();
        return await workerContext.Service.GetDeviceList(options);
    }

    public async Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        if (_scanningContext.WorkerFactory == null)
        {
            throw new InvalidOperationException();
        }
        using var workerContext = _scanningContext.WorkerFactory.Create();
        await workerContext.Service.TwainScan(options, cancelToken, twainEvents);
    }
}