using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

public interface ITwainController
{
    Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken);
}