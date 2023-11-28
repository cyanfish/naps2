using System.Globalization;
using System.Threading;
using NAPS2.Escl;
using NAPS2.Escl.Server;
using NAPS2.Scan;

namespace NAPS2.Remoting.Server;

internal class ScanJob : IEsclScanJob
{
    private readonly ScanController _controller;
    private readonly CancellationTokenSource _cts = new();
    private readonly IAsyncEnumerator<ProcessedImage> _enumerable;
    private readonly TaskCompletionSource<bool> _completedTcs = new();
    private Action<JobStatus>? _callback;
    private bool _hasError;

    public ScanJob(ScanController controller, Driver driver, ScanDevice device, EsclScanSettings settings)
    {
        _controller = controller;
        _controller.ScanEnd += (_, _) =>
        {
            _callback?.Invoke(_hasError ? JobStatus.Aborted : JobStatus.Completed);
            _completedTcs.TrySetResult(!_hasError);
        };
        _controller.ScanError += (_, _) => { _hasError = true; };
        var options = new ScanOptions
        {
            Driver = driver,
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
        _enumerable = controller.Scan(options, _cts.Token).GetAsyncEnumerator();
    }

    public void Cancel()
    {
        _cts.Cancel();
        _callback?.Invoke(JobStatus.Canceled);
    }

    public void RegisterStatusChangeCallback(Action<JobStatus> callback)
    {
        _callback = callback;
    }

    // TODO: Handle errors
    public async Task<bool> WaitForNextDocument() => await _enumerable.MoveNextAsync();

    public void WriteDocumentTo(Stream stream)
    {
        // TODO: PDF etc
        _enumerable.Current.Save(stream, ImageFileFormat.Jpeg);
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