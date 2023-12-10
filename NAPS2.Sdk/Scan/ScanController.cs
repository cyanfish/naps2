using System.Threading;
using NAPS2.Ocr;
using NAPS2.Scan.Internal;

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

    /// <summary>
    /// Initializes a new instance of the ScanController class with the specified ScanningContext and a custom
    /// OcrController. This is generally unnecessary if all you want to do is run OCR as part of exporting a PDF - in
    /// that case you only need to set ScanningContext.OcrEngine and specify OcrParams when exporting.
    /// </summary>
    /// <param name="scanningContext"></param>
    /// <param name="ocrController"></param>
    public ScanController(ScanningContext scanningContext, OcrController ocrController)
        : this(scanningContext, new LocalPostProcessor(scanningContext, ocrController), new ScanOptionsValidator(),
            new ScanBridgeFactory(scanningContext))
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
        void PageStartCallback() => PageStart?.Invoke(this, new PageStartEventArgs(++pageNumber));

        void PageProgressCallback(double progress) =>
            PageProgress?.Invoke(this, new PageProgressEventArgs(pageNumber, progress));

        void PageEndCallback(ProcessedImage image) => PageEnd?.Invoke(this, new PageEndEventArgs(pageNumber, image));

        ScanStartCallback();
        return AsyncProducers.RunProducer<ProcessedImage>(async produceImage =>
        {
            try
            {
                var bridge = _scanBridgeFactory.Create(options);
                await bridge.Scan(options, cancelToken, new ScanEvents(PageStartCallback, PageProgressCallback),
                    (image, postProcessingContext) =>
                    {
                        image = _localPostProcessor.PostProcess(image, options, postProcessingContext);
                        produceImage(image);
                        PageEndCallback(image);
                    });
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