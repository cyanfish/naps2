using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

// TODO: We still need to write the temp image in the remote post processor (where possible... what about actual remote?)
/// <summary>
/// Triggers OCR during the scanning process. To use this: create an instance, set the properties appropriately, and
/// pass it as a constructor argument to ScanController. Then OCR will be automatically triggered on each scanned page.
///
/// TODO: Make this more friendlier to use from an SDK perspective, e.g. changing up the events, or maybe even adding an option to include results in postprocessingdata (and delay the scanned image production)
/// TODO: Maybe even have a OCR-package-provided subclass that automatically sets some properties.
/// TODO: Also document each property.
/// TODO: And maybe rethink the class structure a bit to separate the user-friendly portion of this class (basic configuration and maybe events) from the more nitty gritty details. 
///
/// In the NAPS2 desktop application, the OcrController instance is registered on OcrOperationManager to show progress.
/// OCR results are not accessed directly - OCR is only done to populate the OcrRequestQueue cache for future Save PDF
/// operations.
/// </summary>
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

    // TODO: PdfSharpExporter gets the engine from ScanningContext. Should we make this consistent?
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