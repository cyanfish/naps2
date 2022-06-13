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

    public string? LanguageCode { get; set; }

    public OcrMode Mode { get; set; }

    public double TimeoutInSeconds { get; set; }

    public bool AttachPerImageCancellation { get; set; }

    public OcrPriority Priority { get; set; }

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
        if (LanguageCode == null)
        {
            throw new InvalidOperationException("OCR is enabled but no language code is specified.");
        }
        if (!Engine.CanProcess(LanguageCode))
        {
            throw new InvalidOperationException("OCR is enabled but the engine can't handle the specified language.");
        }

        CancellationTokenSource cts = new CancellationTokenSource();
        _cancellationTokenSources.Add(image.GetWeakReference(), cts);

        if (AttachPerImageCancellation)
        {
            image = image.WithPostProcessingData(image.PostProcessingData with { OcrCts = cts }, true);
        }
        
        return _scanningContext.OcrRequestQueue.Enqueue(
            Engine,
            image,
            tempImageFilePath,
            new OcrParams(LanguageCode, Mode, TimeoutInSeconds),
            Priority,
            cts.Token);
    }

    public void CancelAll()
    {
        foreach (var cts in _cancellationTokenSources.Values)
        {
            cts.Cancel();
        }
    }
}