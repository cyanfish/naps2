using System.Threading;

namespace NAPS2.Scan;

/// <summary>
/// A high-level interface used for scanning.
/// This abstracts away the logic of obtaining and using an instance of IScanDriver.
/// </summary>
public interface IScanPerformer
{
    Task<DeviceChoice> PromptForDevice(ScanProfile scanProfile, bool allowAlwaysAsk = true, IntPtr dialogParent = default);

    IAsyncEnumerable<ScanDevice> GetDevices(ScanProfile scanProfile, CancellationToken cancelToken = default);

    Task<ScanCaps> GetCaps(ScanProfile scanProfile, CancellationToken cancelToken = default);

    IAsyncEnumerable<ProcessedImage> PerformScan(ScanProfile scanProfile, ScanParams scanParams, IntPtr dialogParent = default, CancellationToken cancelToken = default);
}