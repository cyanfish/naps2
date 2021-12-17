using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Scan.Internal;

namespace NAPS2.Scan;

public class ScanController : IScanController
{
    private readonly ILocalPostProcessor _localPostProcessor;
    private readonly ScanOptionsValidator _scanOptionsValidator;
    private readonly IScanBridgeFactory _scanBridgeFactory;

    public ScanController()
        : this(new LocalPostProcessor(), new ScanOptionsValidator(), new ScanBridgeFactory())
    {
    }

    internal ScanController(ILocalPostProcessor localPostProcessor, ScanOptionsValidator scanOptionsValidator, IScanBridgeFactory scanBridgeFactory)
    {
        _localPostProcessor = localPostProcessor;
        _scanOptionsValidator = scanOptionsValidator;
        _scanBridgeFactory = scanBridgeFactory;
    }

    public Task<List<ScanDevice>> GetDeviceList() => GetDeviceList(new ScanOptions());

    public Task<List<ScanDevice>> GetDeviceList(Driver driver) => GetDeviceList(new ScanOptions { Driver = driver });

    public async Task<List<ScanDevice>> GetDeviceList(ScanOptions options)
    {
        options = _scanOptionsValidator.ValidateAll(options);
        var bridge = _scanBridgeFactory.Create(options);
        return await bridge.GetDeviceList(options);
    }

    public ScannedImageSource Scan(ScanOptions options, CancellationToken cancelToken = default)
    {
        options = _scanOptionsValidator.ValidateAll(options);
        var sink = new ScannedImageSink();
        int pageNumber = 0;

        void ScanStartCallback() => ScanStart?.Invoke(this, new ScanStartEventArgs());
        void ScanEndCallback(ScannedImageSource source) => ScanEnd?.Invoke(this, new ScanEndEventArgs(source));
        void ScanErrorCallback(Exception ex) => ScanError?.Invoke(this, new ScanErrorEventArgs(ex));
        void PageStartCallback() => PageStart?.Invoke(this, new PageStartEventArgs(++pageNumber));
        void PageProgressCallback(double progress) => PageProgress?.Invoke(this, new PageProgressEventArgs(pageNumber, progress));
        void PageEndCallback(ScannedImage image) => PageEnd?.Invoke(this, new PageEndEventArgs(pageNumber, image));

        ScanStartCallback();
        Task.Run(async () =>
        {
            try
            {
                var bridge = _scanBridgeFactory.Create(options);
                await bridge.Scan(options, cancelToken, new ScanEvents(PageStartCallback, PageProgressCallback),
                    (scannedImage, postProcessingContext) =>
                    {
                        _localPostProcessor.PostProcess(scannedImage, options, postProcessingContext);
                        sink.PutImage(scannedImage);
                        PageEndCallback(scannedImage);
                    });
                sink.SetCompleted();
            }
            catch (Exception ex)
            {
                sink.SetError(ex);
                ScanErrorCallback(ex);
            }
            ScanEndCallback(sink.AsSource());
        });
        return sink.AsSource();
    }

    public event EventHandler<ScanStartEventArgs>? ScanStart;

    public event EventHandler<ScanEndEventArgs>? ScanEnd;

    public event EventHandler<ScanErrorEventArgs>? ScanError;

    public event EventHandler<PageStartEventArgs>? PageStart;

    public event EventHandler<PageProgressEventArgs>? PageProgress;

    public event EventHandler<PageEndEventArgs>? PageEnd;
}