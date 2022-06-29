using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

public class RemoteTwainController : ITwainController
{
    private readonly ScanningContext _scanningContext;

    public RemoteTwainController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
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