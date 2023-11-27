using System.Threading;

namespace NAPS2.Scan.Internal;

internal class RemoteScanController : IRemoteScanController
{
    private readonly IScanDriverFactory _scanDriverFactory;
    private readonly IRemotePostProcessor _remotePostProcessor;

    public RemoteScanController(ScanningContext scanningContext)
        : this(new ScanDriverFactory(scanningContext), new RemotePostProcessor(scanningContext))
    {
    }

    public RemoteScanController(IScanDriverFactory scanDriverFactory, IRemotePostProcessor remotePostProcessor)
    {
        _scanDriverFactory = scanDriverFactory;
        _remotePostProcessor = remotePostProcessor;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        await _scanDriverFactory.Create(options).GetDevices(options, cancelToken, device =>
        {
            var skipWiaDevices = options.Driver == Driver.Twain && !options.TwainOptions.IncludeWiaDevices;
            if (skipWiaDevices && device.ID.StartsWith("WIA-", StringComparison.InvariantCulture))
            {
                return;
            }
            if (options.Driver == Driver.Escl)
            {
                if (options.EsclOptions.ExcludeUuids.Contains(Escl.EsclScanDriver.GetUuid(device)))
                {
                    return;
                }
            }

            callback(device);
        });
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<ProcessedImage, PostProcessingContext> callback)
    {
        var driver = _scanDriverFactory.Create(options);
        var progressThrottle = new EventThrottle<double>(scanEvents.PageProgress);
        var driverScanEvents = new ScanEvents(scanEvents.PageStart, progressThrottle.OnlyIfChanged);
        int pageNumber = 0;
        await driver.Scan(options, cancelToken, driverScanEvents, image =>
        {
            var postProcessingContext = new PostProcessingContext
            {
                // Note we still want to increment even if we don't send a page callback (i.e. when blank detection is
                // on). The page number is only used to determine whether we're on the front or back of a duplex scan. 
                PageNumber = ++pageNumber
            };
            var scannedImage = _remotePostProcessor.PostProcess(image, options, postProcessingContext);
            if (scannedImage != null)
            {
                callback(scannedImage, postProcessingContext);
            }
        });
    }
}