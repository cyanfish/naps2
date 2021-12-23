using System.Threading;

namespace NAPS2.Scan.Internal;

/// <summary>
/// Delegates to an implementation of IScanDriver based on the options and environment.
/// </summary>
internal interface IRemoteScanController
{
    Task<List<ScanDevice>> GetDeviceList(ScanOptions options);

    Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<RenderableImage, PostProcessingContext> callback);
}