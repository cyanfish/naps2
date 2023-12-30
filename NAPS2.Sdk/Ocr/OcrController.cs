using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

/// <summary>
/// Provides fine-grained control over OCR during the scanning process. This can be used as an optional constructor
/// argument to ScanController. This is independent of ScanOptions.OcrParams which determines whether OCR is actually
/// performed.
///
/// In the NAPS2 desktop application, the OcrController instance is registered on OcrOperationManager to show progress.
/// OCR results are not accessed directly - OCR is only done to populate the OcrRequestQueue cache for future Save PDF
/// operations.
/// </summary>
// TODO: This model seems overly complicated - can we do something simpler like having a singleton OcrController on
// the ScanningContext?
internal class OcrController
{
    private readonly ScanningContext _scanningContext;
    private readonly Dictionary<ProcessedImage.WeakReference, CancellationTokenSource>
        _cancellationTokenSources = new();

    public OcrController(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    /// <summary>
    /// Fired when an individual OCR operation (i.e. one page) is started on this controller.
    /// </summary>
    public event EventHandler<OcrEventArgs>? OcrStarted;

    /// <summary>
    /// Fired when an individual OCR operation (i.e. one page) that was started on this controller finishes
    /// (successfully or unsuccessfully).
    /// </summary>
    public event EventHandler<OcrEventArgs>? OcrCompleted;

    public Task<OcrResult?> Start(ref ProcessedImage image, string tempImageFilePath, OcrParams ocrParams,
        OcrPriority ocrPriority)
    {
        if (string.IsNullOrEmpty(ocrParams.LanguageCode))
        {
            throw new InvalidOperationException("OCR is disabled.");
        }
        var engine = _scanningContext.OcrEngine;
        if (engine == null)
        {
            throw new InvalidOperationException("OCR is enabled but no OCR engine is set on ScanningContext.");
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        _cancellationTokenSources.Add(image.GetWeakReference(), cts);
        image = image.WithPostProcessingData(image.PostProcessingData with { OcrCts = cts }, true);

        var task = _scanningContext.OcrRequestQueue.Enqueue(
            _scanningContext,
            engine,
            image,
            tempImageFilePath,
            ocrParams,
            ocrPriority,
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