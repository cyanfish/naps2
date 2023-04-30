using System.Threading;

namespace NAPS2.Scan.Internal.Twain;

/// <summary>
/// Stub implementation of ITwainSessionController for unsupported platforms.
/// </summary>
internal class StubTwainSessionController : ITwainSessionController
{
    public Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        throw new NotSupportedException();
    }

    public Task StartScan(ScanOptions options, ITwainEvents twainEvents, CancellationToken cancelToken)
    {
        throw new NotSupportedException();
    }
}