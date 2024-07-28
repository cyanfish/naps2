using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Ocr;
using NAPS2.Scan.Exceptions;
using NAPS2.Scan.Internal;
using NAPS2.Scan.Internal.Sane;

namespace NAPS2.Scan;

/// <summary>
/// The main entry point for scanning with NAPS2.
/// </summary>
public class ScanController
{
    private readonly ScanningContext _scanningContext;
    private readonly ILocalPostProcessor _localPostProcessor;
    private readonly ScanOptionsValidator _scanOptionsValidator;
    private readonly IScanBridgeFactory _scanBridgeFactory;

    /// <summary>
    /// Initializes a new instance of the ScanController class with the specified ScanningContext.
    /// </summary>
    /// <param name="scanningContext"></param>
    public ScanController(ScanningContext scanningContext)
        : this(scanningContext, new LocalPostProcessor(scanningContext, new OcrController(scanningContext)),
            new ScanOptionsValidator(),
            new ScanBridgeFactory(scanningContext))
    {
    }

    internal ScanController(ScanningContext scanningContext, IScanBridgeFactory scanBridgeFactory)
        : this(scanningContext, new LocalPostProcessor(scanningContext, new OcrController(scanningContext)),
            new ScanOptionsValidator(), scanBridgeFactory)
    {
    }

    internal ScanController(ScanningContext scanningContext, ILocalPostProcessor localPostProcessor,
        ScanOptionsValidator scanOptionsValidator, IScanBridgeFactory scanBridgeFactory)
    {
        _scanningContext = scanningContext;
        _localPostProcessor = localPostProcessor;
        _scanOptionsValidator = scanOptionsValidator;
        _scanBridgeFactory = scanBridgeFactory;
    }

    /// <summary>
    /// Gets a list of devices using the default driver for the system (WIA on Windows, Apple on Mac, SANE on Linux).
    /// </summary>
    /// <returns>The device list.</returns>
    public Task<List<ScanDevice>> GetDeviceList() => GetDeviceList(new ScanOptions());

    /// <summary>
    /// Gets a list of devices using the specified driver.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <returns>The device list.</returns>
    public Task<List<ScanDevice>> GetDeviceList(Driver driver) => GetDeviceList(new ScanOptions { Driver = driver });

    /// <summary>
    /// Gets a list of devices using the specified options. This is mainly just the driver but some other properties
    /// may affect the list of devices (e.g. TwainOptions.Dsm).
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <returns>The device list.</returns>
    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        options = _scanOptionsValidator.ValidateAll(options, _scanningContext, false);
        var bridge = _scanBridgeFactory.Create(options);
        var devices = new List<ScanDevice>();
        await bridge.GetDevices(options, CancellationToken.None, devices.Add);
        return devices;
    }

    /// <summary>
    /// Gets an enumerable of devices using the default driver for the system (WIA on Windows, Apple on Mac, SANE on
    /// Linux). Depending on the driver this may yield devices incrementally or all at once.
    /// </summary>
    /// <param name="cancelToken">A token used to cancel the operation.</param>
    /// <returns>The device enumerable.</returns>
    public IAsyncEnumerable<ScanDevice> GetDevices(CancellationToken cancelToken = default) =>
        GetDevices(new ScanOptions(), cancelToken);

    /// <summary>
    /// Gets an enumerable of devices using the specified driver. Depending on the driver this may yield devices
    /// incrementally or all at once.
    /// </summary>
    /// <param name="driver">The driver to use.</param>
    /// <param name="cancelToken">A token used to cancel the operation.</param>
    /// <returns>The device enumerable.</returns>
    public IAsyncEnumerable<ScanDevice> GetDevices(Driver driver, CancellationToken cancelToken = default) =>
        GetDevices(new ScanOptions { Driver = driver }, cancelToken);

    /// <summary>
    /// Gets an enumerable of devices using the specified options. This is mainly just the driver but some other
    /// properties may affect the list of devices (e.g. TwainOptions.Dsm). Depending on the driver this may yield
    /// devices incrementally or all at once.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <param name="cancelToken">A token used to cancel the operation.</param>
    /// <returns>The device enumerable.</returns>
    public IAsyncEnumerable<ScanDevice> GetDevices(ScanOptions options, CancellationToken cancelToken = default)
    {
        options = _scanOptionsValidator.ValidateAll(options, _scanningContext, false);
        var bridge = _scanBridgeFactory.Create(options);
        return AsyncProducers.RunProducer<ScanDevice>(async produce =>
        {
            await bridge.GetDevices(options, cancelToken, produce);
        });
    }

    public Task<ScanCaps> GetCaps(ScanDevice device, CancellationToken cancelToken = default) =>
        GetCaps(new ScanOptions { Device = device }, cancelToken);

    public async Task<ScanCaps> GetCaps(ScanOptions options, CancellationToken cancelToken = default)
    {
        options = _scanOptionsValidator.ValidateAll(options, _scanningContext, true);
        var bridge = _scanBridgeFactory.Create(options);
        return await bridge.GetCaps(options, cancelToken);
    }

    /// <summary>
    /// Scans using the specified options and returns an enumerable of the scanned images.
    /// <para/>
    /// If PropagateErrors is true (the default), enumerating the result will result in an error if there was a problem
    /// scanning. For example, if one page was successfully scanned and then there was a paper jam, the first scanned
    /// image will be yielded and then an exception will be thrown for the paper jam. Or if the scanner was offline an
    /// exception will be thrown as soon as the caller tries to start enumerating the results.
    /// <para/>
    /// You can get detailed progress information by subscribing to the relevant ScanController events.
    /// </summary>
    /// <param name="options">The options to use.</param>
    /// <param name="cancelToken">A token used to cancel the operation.</param>
    /// <returns>The scanned images enumerable.</returns>
    public IAsyncEnumerable<ProcessedImage> Scan(ScanOptions options, CancellationToken cancelToken = default)
    {
        options = _scanOptionsValidator.ValidateAll(options, _scanningContext, true);
        int pageNumber = 0;

        Exception? scanError = null;
        void ScanStartCallback() => ScanStart?.Invoke(this, EventArgs.Empty);
        void ScanEndCallback() => ScanEnd?.Invoke(this, new ScanEndEventArgs(scanError));
        void PageStartCallback()
        {
            pageNumber++;
            PageStart?.Invoke(this, new PageStartEventArgs(pageNumber));
        }
        void PageProgressCallback(double progress) =>
            PageProgress?.Invoke(this, new PageProgressEventArgs(pageNumber, progress));
        void PageEndCallback(ProcessedImage image) => PageEnd?.Invoke(this, new PageEndEventArgs(pageNumber, image));

        _scanningContext.Logger.LogDebug("Scanning with {Device}", options.Device);
        _scanningContext.Logger.LogDebug("Scan source: {Source}; bit depth: {BitDepth}; dpi: {Dpi}; page size: {PageSize}",
            options.PaperSource, options.BitDepth, options.Dpi, options.PageSize);
        ScanStartCallback();
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            var bridge = _scanBridgeFactory.Create(options);

            async Task DoScan(ScanOptions actualOptions)
            {
                await bridge.Scan(actualOptions, cancelToken, new ScanEvents(PageStartCallback, PageProgressCallback),
                    (image, postProcessingContext) =>
                    {
                        image = _localPostProcessor.PostProcess(image, actualOptions, postProcessingContext);
                        produceImage(image);
                        PageEndCallback(image);
                    });
            }

            try
            {
                try
                {
                    await DoScan(options);
                }
                catch (DeviceOfflineException) when
                    (options.Driver == Driver.Sane &&
                     (_scanningContext.WorkerFactory != null || PlatformCompat.System.IsLibUsbReliable))
                {
                    // Some SANE backends (e.g. airscan, genesys) have inconsistent IDs so "device offline" might actually
                    // just mean "device id has changed". We can query for a device that matches the name of the
                    // original device, and assume it's the same physical device, which should generally be correct.
                    //
                    // TODO: Ideally this would be contained within SaneScanDriver, but due to libusb's unreliability on
                    // macOS, we have to make sure each call is in a separate worker process. Makes me wonder if the
                    // scanning pipeline could be redesigned so that drivers have more control over worker processes.
                    _scanningContext.Logger.LogDebug(
                        "SANE Device appears offline; re-querying in case of ID change for name \"{Name}\"",
                        options.Device!.Name);
                    var getDevicesOptions = options.Clone();
                    getDevicesOptions.SaneOptions.Backend = SaneScanDriver.GetBackend(options.Device!);
                    ScanDevice? matchingDevice = null;
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
                    await bridge.GetDevices(getDevicesOptions, cts.Token, device =>
                    {
                        if (device.Name == options.Device!.Name)
                        {
                            matchingDevice = device;
                            cts.Cancel();
                        }
                    });
                    if (matchingDevice == null)
                    {
                        _scanningContext.Logger.LogDebug("No matching device found");
                        throw;
                    }
                    var actualOptions = options.Clone();
                    actualOptions.Device = matchingDevice;
                    await DoScan(actualOptions);
                }
            }
            catch (Exception ex)
            {
                scanError = ex;
                if (PropagateErrors)
                {
                    throw;
                }
            }
            finally
            {
                ScanEndCallback();
            }
        });
    }

    /// <summary>
    /// Whether scan errors should be thrown when enumerating the IAsyncEnumerable result. True by default. If you set
    /// this to false, you will need to listen for the ScanError event to handle errors.
    /// </summary>
    public bool PropagateErrors { get; set; } = true;

    /// <summary>
    /// Occurs when a Scan operation is about to start.
    /// </summary>
    public event EventHandler? ScanStart;

    /// <summary>
    /// Occurs when a Scan operation has completed. If the scan ends due to an error, the Error property will be set on
    /// the event args. For more detailed diagnostics, set ScanningContext.Logger.
    /// </summary>
    public event EventHandler<ScanEndEventArgs>? ScanEnd;

    /// <summary>
    /// Occurs when scanning starts for a page. This can be called multiple times during a single Scan operation if it
    /// is a feeder scanner.
    /// </summary>
    public event EventHandler<PageStartEventArgs>? PageStart;

    /// <summary>
    /// Occurs when the progress changes for scanning a page.
    /// </summary>
    public event EventHandler<PageProgressEventArgs>? PageProgress;

    /// <summary>
    /// Occurs when scanning is done for a page and the scanned image is available. This can be called multiple times
    /// during a single Scan operation if it is a feeder scanner.
    /// </summary>
    public event EventHandler<PageEndEventArgs>? PageEnd;
}