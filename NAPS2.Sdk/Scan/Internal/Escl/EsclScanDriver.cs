using System.Threading;
using Microsoft.Extensions.Logging;
using NAPS2.Escl;
using NAPS2.Escl.Client;
using NAPS2.Pdf;
using NAPS2.Scan.Exceptions;

namespace NAPS2.Scan.Internal.Escl;

internal class EsclScanDriver : IScanDriver
{
    private readonly ScanningContext _scanningContext;

    public EsclScanDriver(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public async Task GetDevices(ScanOptions options, CancellationToken cancelToken, Action<ScanDevice> callback)
    {
        // TODO: Run location in a persistent background service
        using var locator = new EsclServiceLocator(service =>
        {
            // Store both the IP and UUID so we can preferentially find by the IP, but also fall back to looking for
            // the UUID in case the IP changed
            var ip = service.IpV4 ?? service.IpV6;
            var id = $"{ip}|{service.Uuid}";
            var name = string.IsNullOrEmpty(service.ScannerName)
                ? $"{ip}"
                : $"{service.ScannerName} ({ip})";
            callback(new ScanDevice(id, name));
        });
        locator.Start();
        try
        {
            await Task.Delay(2000, cancelToken);
        }
        catch (TaskCanceledException)
        {
        }
    }

    public async Task Scan(ScanOptions options, CancellationToken cancelToken, IScanEvents scanEvents,
        Action<IMemoryImage> callback)
    {
        if (cancelToken.IsCancellationRequested) return;

        var foundTcs = new TaskCompletionSource<EsclService?>();
        using var locator = new EsclServiceLocator(async service =>
        {
            var parts = options.Device!.ID.Split('|');
            var ip = parts[0];
            var uuid = parts[1];
            if ((service.IpV4 ?? service.IpV6!).ToString() == ip)
            {
                foundTcs.TrySetResult(service);
            }
            else
            {
                var client = new EsclClient(service);
                try
                {
                    var caps = await client.GetCapabilities();
                    if (caps.Uuid == uuid)
                    {
                        foundTcs.TrySetResult(service);
                    }
                }
                catch (Exception ex)
                {
                    _scanningContext.Logger.LogDebug(ex, "ESCL error");
                }
            }
        });
        Task.Delay(2000).ContinueWith(_ => foundTcs.TrySetResult(null)).AssertNoAwait();
        locator.Start();
        var service = await foundTcs.Task ?? throw new DeviceException(SdkResources.DeviceOffline);

        if (cancelToken.IsCancellationRequested) return;

        var client = new EsclClient(service);
        client.Logger = _scanningContext.Logger;
        var caps = await client.GetCapabilities();
        var status = await client.GetStatus();

        if (cancelToken.IsCancellationRequested) return;

        var job = await client.CreateScanJob(new EsclScanSettings
        {
            Width = (int) Math.Round(options.PageSize!.WidthInInches * 300),
            Height = (int) Math.Round(options.PageSize!.HeightInInches * 300),
            XResolution = options.Dpi, // TODO: Match to caps
            YResolution = options.Dpi,
            ColorMode = options.BitDepth switch
            {
                BitDepth.Color => EsclColorMode.RGB24,
                BitDepth.Grayscale => EsclColorMode.Grayscale8,
                BitDepth.BlackAndWhite => EsclColorMode.BlackAndWhite1,
                _ => EsclColorMode.RGB24
            },
            InputSource = options.PaperSource switch
            {
                PaperSource.Feeder or PaperSource.Duplex => EsclInputSource.Feeder,
                _ => EsclInputSource.Platen
            },
            Duplex = options.PaperSource == PaperSource.Duplex,
            DocumentFormat = options.BitDepth == BitDepth.BlackAndWhite || options.MaxQuality
                ? "application/pdf" // TODO: Use PNG if available?
                : "image/jpeg"
            // TODO: Offset, brightness/contrast, quality, etc.
        });

        var cancelOnce = new Once(() => client.CancelJob(job).AssertNoAwait());
        using var cancelReg = cancelToken.Register(cancelOnce.Run);

        try
        {

            while (true)
            {
                scanEvents.PageStart();
                RawDocument? doc;
                try
                {
                    // TODO: PDF or jpeg?
                    doc = await client.NextDocument(job);
                }
                catch (Exception ex)
                {
                    _scanningContext.Logger.LogDebug(ex, "ESCL error");
                    break;
                }
                if (doc == null) break;
                if (doc.ContentType == "application/pdf")
                {
                    // TODO: For SDK some kind an error message if Pdfium isn't present
                    var renderer = new PdfiumPdfRenderer();
                    foreach (var image in renderer.Render(_scanningContext.ImageContext, doc.Data, doc.Data.Length,
                                 PdfRenderSize.FromDpi(options.Dpi)))
                    {
                        callback(image);
                    }
                }
                else
                {
                    callback(_scanningContext.ImageContext.Load(doc.Data));
                }
            }
        }
        finally
        {
            cancelOnce.Run();
        }
    }
}