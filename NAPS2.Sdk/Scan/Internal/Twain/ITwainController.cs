using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Interface for interacting with a Twain session that might happen in the current (local) process or a worker (remote)
/// process. 
/// </summary>
internal interface ITwainController
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);
    Task<ScanCaps?> GetCaps(ScanOptions options);
    Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken);
}