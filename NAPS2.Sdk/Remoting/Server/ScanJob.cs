using System.Globalization;
using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

internal class ScanJob : IEsclScanJob
{
    private const string CT_PDF = "application/pdf";
    private const string CT_PNG = "image/png";
    private const string CT_JPEG = "image/jpeg";

    private readonly ScanningContext _scanningContext;
    private readonly ScanController _controller;
    private readonly CancellationTokenSource _cts = new();
    private readonly IAsyncEnumerator<ProcessedImage> _enumerable;
    private readonly TaskCompletionSource<bool> _completedTcs = new();
    private readonly List<ProcessedImage> _pdfImages = [];
    private Action<StatusTransition>? _callback;
    private bool _hasError;

    public ScanJob(ScanningContext scanningContext, ScanController controller, ScanDevice device,
        EsclScanSettings settings)
    {
        _scanningContext = scanningContext;
        _controller = controller;
        _controller.ScanEnd += (_, _) =>
        {
            _callback?.Invoke(StatusTransition.DeviceIdle);
            if (_hasError)
            {
                _callback?.Invoke(StatusTransition.AbortJob);
            }
            _completedTcs.TrySetResult(!_hasError);
        };
        _controller.ScanError += (_, _) => { _hasError = true; };
        var options = new ScanOptions
        {
            Device = device,
            Dpi = Math.Max(settings.XResolution, settings.YResolution),
            BitDepth = settings.ColorMode switch
            {
                EsclColorMode.BlackAndWhite1 => BitDepth.BlackAndWhite,
                EsclColorMode.Grayscale8 or EsclColorMode.Grayscale16 => BitDepth.Grayscale,
                _ => BitDepth.Color
            },
            PaperSource = (settings.InputSource, settings.Duplex) switch
            {
                (EsclInputSource.Feeder, false) => PaperSource.Feeder,
                (EsclInputSource.Feeder, true) => PaperSource.Duplex,
                _ => PaperSource.Flatbed
            },
            PageSize = settings.Width > 0 && settings.Height > 0
                ? new PageSize(settings.Width / 300m, settings.Height / 300m, PageSizeUnit.Inch)
                : PageSize.Letter,
            // TODO: Align based on offset, etc.
        };
        var requestedFormat = settings.DocumentFormat;
        ContentType = requestedFormat switch
        {
            CT_PNG or CT_PDF => requestedFormat,
            _ => CT_JPEG
        };
        try
        {
            _enumerable = controller.Scan(options, _cts.Token).GetAsyncEnumerator();
        }
        catch (Exception)
        {
            _callback?.Invoke(StatusTransition.DeviceIdle);
            _callback?.Invoke(StatusTransition.AbortJob);
            throw;
        }
    }

    public string ContentType { get; }

    public void Cancel()
    {
        _cts.Cancel();
        _callback?.Invoke(StatusTransition.CancelJob);
    }

    public void RegisterStatusTransitionCallback(Action<StatusTransition> callback)
    {
        _callback = callback;
    }

    public async Task<bool> WaitForNextDocument()
    {
        if (ContentType == CT_PDF)
        {
            // For PDFs we merge all the pages into a single PDF document, so we need to wait for the full scan here
            if (!await _enumerable.MoveNextAsync())
            {
                return false;
            }
            do
            {
                _pdfImages.Add(_enumerable.Current);
            } while (await _enumerable.MoveNextAsync());
            return true;
        }

        return await _enumerable.MoveNextAsync();
    }

    public async Task WriteDocumentTo(Stream stream)
    {
        if (ContentType == CT_JPEG)
        {
            _enumerable.Current.Save(stream, ImageFileFormat.Jpeg);
        }
        if (ContentType == CT_PNG)
        {
            _enumerable.Current.Save(stream, ImageFileFormat.Png);
        }
        if (ContentType == CT_PDF)
        {
            var pdfExporter = new PdfExporter(_scanningContext);
            await pdfExporter.Export(stream, _pdfImages);
        }
    }

    public async Task WriteProgressTo(Stream stream)
    {
        if (_completedTcs.Task.IsCompleted)
        {
            return;
        }

        var pageEndTcs = new TaskCompletionSource<bool>();
        var streamWriter = new StreamWriter(stream);

        void OnPageProgress(object? sender, PageProgressEventArgs e)
        {
            streamWriter.WriteLine(e.Progress.ToString(CultureInfo.InvariantCulture));
            streamWriter.Flush();
        }

        void OnPageEnd(object? sender, PageEndEventArgs e)
        {
            pageEndTcs.TrySetResult(true);
        }

        _controller.PageProgress += OnPageProgress;
        _controller.PageEnd += OnPageEnd;
        await Task.WhenAny(pageEndTcs.Task, _completedTcs.Task);
        _controller.PageProgress -= OnPageProgress;
        _controller.PageEnd -= OnPageEnd;
    }
}