using System.Threading;

namespace NAPS2.Scan;

/// <summary>
/// A high-level interface used for scanning.
/// This abstracts away the logic of obtaining and using an instance of IScanDriver.
/// </summary>
public interface IScanPerformer
{
    Task<ScanDevice?> PromptForDevice(ScanProfile scanProfile, IntPtr dialogParent = default);

    IAsyncEnumerable<ProcessedImage> PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default);
}