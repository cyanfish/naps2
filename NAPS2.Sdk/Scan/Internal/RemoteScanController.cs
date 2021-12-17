using System.Threading;

namespace NAPS2.Scan.Internal;

internal class RemoteScanController : IRemoteScanController
{
    private readonly IScanDriverFactory _scanDriverFactory;
    private readonly IRemotePostProcessor _remotePostProcessor;

    public RemoteScanController()
        : this(new ScanDriverFactory(ImageContext.Default), new RemotePostProcessor())
    {
    }

    public RemoteScanController(ImageContext imageContext)
        : this(new ScanDriverFactory(imageContext), new RemotePostProcessor(imageContext))
    {
    }

    public RemoteScanController(IScanDriverFactory scanDriverFactory, IRemotePostProcessor remotePostProcessor)
    {
        _scanDriverFactory = scanDriverFactory;
        _remotePostProcessor = remotePostProcessor;
    }

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        var deviceList = await _scanDriverFactory.Create(options).GetDeviceList(options);
        if (options.Driver == Driver.Twain && !options.TwainOptions.IncludeWiaDevices)
        {
            deviceList = deviceList.Where(x => !x.ID.StartsWith("WIA-", StringComparison.InvariantCulture)).ToList();
        }
        return deviceList;
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents, Action<ScannedImage, PostProcessingContext> callback)
    {
        var driver = _scanDriverFactory.Create(options);
        var progressThrottle = new EventThrottle<double>(scanEvents.PageProgress);
        var driverScanEvents = new ScanEvents(scanEvents.PageStart, progressThrottle.OnlyIfChanged);
        int pageNumber = 0;
        await driver.Scan(options, cancelToken, driverScanEvents, image =>
        {
            var postProcessingContext = new PostProcessingContext
            {
                PageNumber = ++pageNumber
            };
            var scannedImage = _remotePostProcessor.PostProcess(image, options, postProcessingContext);
            callback(scannedImage, postProcessingContext);
        });
    }
}