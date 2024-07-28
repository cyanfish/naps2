using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Delegates to an implementation of IScanDriver based on the options and environment.
/// </summary>
internal interface IRemoteScanController
{
    Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback);

    Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken);

    Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ProcessedImage, PostProcessingContext> callback);
}