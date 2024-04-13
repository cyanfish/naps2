using System.Globalization;
using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Pdf;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Remoting.Server;

internal class ScanJob : IEsclScanJob
{
    private readonly ScanningContext _scanningContext;
    private readonly ScanController _controller;

    private readonly CancellationTokenSource _cts = new();
    private readonly TaskCompletionSource<bool> _completedTcs = new();
    private readonly IAsyncEnumerator<ProcessedImage> _enumerable;
    private readonly List<ProcessedImage> _allImages = [];
    private readonly List<ProcessedImage> _pdfImages = [];
    private readonly Dictionary<int, double> _lastProgressByPageNumber = new();

    private int _currentPage = 1;
    private Action<StatusTransition>? _statusCallback;
    private Exception? _lastError;
    private Task<bool>? _pausedNextDocumentTask;

    public ScanJob(ScanningContext scanningContext, ScanController controller, ScanDevice device,
        EsclScanSettings settings)
    {
        _scanningContext = scanningContext;
        _controller = controller;
        _controller.PageProgress += (_, args) => _lastProgressByPageNumber[args.PageNumber] = args.Progress;
        _controller.PageEnd += (_, args) =>
        {
            _statusCallback?.Invoke(StatusTransition.PageComplete);
            _allImages.Add(args.Image);
        };
        _controller.ScanEnd += (_, args) =>
        {
            if (args.HasError)
            {
                _lastError = args.Error;
            }
            _statusCallback?.Invoke(StatusTransition.ScanComplete);
            _completedTcs.TrySetResult(!args.HasError);
        };

        var requestedFormat = settings.DocumentFormat;
        ContentType = requestedFormat switch
        {
            ContentTypes.PNG or ContentTypes.PDF => requestedFormat,
            _ => ContentTypes.JPEG
        };
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
            PageAlign = SnapToAlignment(settings.XOffset, settings.Width, EsclInputCaps.DEFAULT_MAX_WIDTH),
            Quality = settings.CompressionFactor ?? ScanOptions.DEFAULT_QUALITY,
            MaxQuality = ContentType == ContentTypes.PNG
        };

        try
        {
            _enumerable = controller.Scan(options, _cts.Token).GetAsyncEnumerator();
        }
        catch (Exception)
        {
            _statusCallback?.Invoke(StatusTransition.AbortJob);
            _statusCallback?.Invoke(StatusTransition.ScanComplete);
            throw;
        }
    }

    private HorizontalAlign SnapToAlignment(int x, int width, int maxWidth)
    {
        if (x == 0 || width >= maxWidth) return HorizontalAlign.Right;
        var fraction = x / (double) (maxWidth - width);
        return fraction switch
        {
            <= 0.25 => HorizontalAlign.Right,
            >= 0.75 => HorizontalAlign.Left,
            _ => HorizontalAlign.Center,
        };
    }

    public string ContentType { get; }

    public void Cancel()
    {
        _cts.Cancel();
        _statusCallback?.Invoke(StatusTransition.CancelJob);
    }

    public void RegisterStatusTransitionCallback(Action<StatusTransition> callback)
    {
        _statusCallback = callback;
    }

    public async Task<bool> WaitForNextDocument(CancellationToken cancelToken)
    {
        Task<bool> nextDocumentTask;
        lock (this)
        {
            nextDocumentTask = _pausedNextDocumentTask ?? Task.Run(async () =>
            {
                if (ContentType == ContentTypes.PDF)
                {
                    // For PDFs we merge all the pages into a single PDF document, so we need to wait for the full scan here
                    if (!await _enumerable.MoveNextAsync())
                    {
                        return false;
                    }
                    do
                    {
                        _currentPage++;
                        _pdfImages.Add(_enumerable.Current);
                    } while (await _enumerable.MoveNextAsync());
                    return true;
                }

                if (await _enumerable.MoveNextAsync())
                {
                    _currentPage++;
                    return true;
                }
                return false;
            });
            _pausedNextDocumentTask = null;
        }
        await Task.WhenAny(nextDocumentTask, cancelToken.WaitHandle.WaitOneAsync());
        lock (this)
        {
            if (!nextDocumentTask.IsCompleted)
            {
                _pausedNextDocumentTask = nextDocumentTask;
                throw new TaskCanceledException();
            }
        }
        return await nextDocumentTask;
    }

    public async Task WriteDocumentTo(Stream stream)
    {
        if (ContentType == ContentTypes.JPEG)
        {
            _enumerable.Current.Save(stream, ImageFileFormat.Jpeg);
            stream.Dispose();
            _enumerable.Current.Dispose();
        }
        if (ContentType == ContentTypes.PNG)
        {
            _enumerable.Current.Save(stream, ImageFileFormat.Png);
            stream.Dispose();
            _enumerable.Current.Dispose();
        }
        if (ContentType == ContentTypes.PDF)
        {
            var pdfExporter = new PdfExporter(_scanningContext);
            await pdfExporter.Export(stream, _pdfImages);
            stream.Dispose();
            foreach (var image in _pdfImages)
            {
                image.Dispose();
            }
            _pdfImages.Clear();
        }
    }

    public async Task WriteProgressTo(Stream stream)
    {
        var pageEndTcs = new TaskCompletionSource<bool>();
        var streamWriter = new StreamWriter(stream);
        var pageNumber = _currentPage;

        void WriteProgress(double progress)
        {
            streamWriter.WriteLine(progress.ToString(CultureInfo.InvariantCulture));
            streamWriter.Flush();
        }
        void OnPageProgress(object? sender, PageProgressEventArgs e)
        {
            if (e.PageNumber == pageNumber)
            {
                WriteProgress(e.Progress);
            }
        }
        void OnPageEnd(object? sender, PageEndEventArgs e) => pageEndTcs.TrySetResult(true);

        if (_lastProgressByPageNumber.TryGetValue(pageNumber, out var lastPageProgress))
        {
            WriteProgress(lastPageProgress);
        }

        _controller.PageProgress += OnPageProgress;
        _controller.PageEnd += OnPageEnd;
        await Task.WhenAny(pageEndTcs.Task, _completedTcs.Task);
        _controller.PageProgress -= OnPageProgress;
        _controller.PageEnd -= OnPageEnd;
    }

    public async Task WriteErrorDetailsTo(Stream stream)
    {
        if (_lastError != null)
        {
            using var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteLineAsync(RemotingHelper.ToError(_lastError).ToXml());
        }
    }

    public void Dispose()
    {
        foreach (var image in _allImages)
        {
            image.Dispose();
        }
    }
}