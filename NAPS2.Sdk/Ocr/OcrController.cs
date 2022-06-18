using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

// TODO: We still need to write the temp image in the remote post processor (where possible... what about actual remote?)
public class OcrController
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<ProcessedImage.WeakReference, CancellationTokenSource>
        _cancellationTokenSources = new();

    public OcrController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public bool EnableOcr { get; set; }

    public IOcrEngine? Engine { get; set; }

    public OcrParams OcrParams { get; set; } = new(null, OcrMode.Default, 0);

    public bool AttachPerImageCancellation { get; set; }

    public OcrPriority Priority { get; set; }

    /// <summary>
    /// Fired when an individual OCR operation (i.e. one page) is started on this controller.
    /// </summary>
    public event EventHandler<OcrEventArgs>? OcrStarted;

    /// <summary>
    /// Fired when an individual OCR operation (i.e. one page) that was started on this controller finishes
    /// (successfully or unsuccessfully).
    /// </summary>
    public event EventHandler<OcrEventArgs>? OcrCompleted;

    public Task<OcrResult?> Start(ref ProcessedImage image, string tempImageFilePath)
    {
        if (!EnableOcr)
        {
            return Task.FromResult<OcrResult?>(null);
        }
        if (Engine == null)
        {
            throw new InvalidOperationException("OCR is enabled but no OCR engine is specified.");
        }
        if (OcrParams.LanguageCode == null)
        {
            throw new InvalidOperationException("OCR is enabled but no language code is specified.");
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        _cancellationTokenSources.Add(image.GetWeakReference(), cts);

        if (AttachPerImageCancellation)
        {
            image = image.WithPostProcessingData(image.PostProcessingData with { OcrCts = cts }, true);
        }

        var task = _scanningContext.OcrRequestQueue.Enqueue(
            Engine,
            image,
            tempImageFilePath,
            OcrParams,
            Priority,
            cts.Token);

        var eventArgs = new OcrEventArgs(task);
        OcrStarted?.Invoke(this, eventArgs);
        task.ContinueWith(t => OcrCompleted?.Invoke(this, eventArgs));

        return task;
    }

    public void CancelAll()
    {
        foreach (var cts in _cancellationTokenSources.Values)
        {
            cts.Cancel();
        }
    }
}