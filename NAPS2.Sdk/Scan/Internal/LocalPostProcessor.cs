using NAPS2.Ocr;

namespace NAPS2.Scan.Internal;

internal class LocalPostProcessor : ILocalPostProcessor
{
    private readonly ScanningContext _scanningContext;
    private readonly OcrController _ocrController;

    public LocalPostProcessor(ScanningContext scanningContext, OcrController ocrController)
    {
        _ocrController = ocrController;
        _scanningContext = scanningContext;
    }

    public ProcessedImage PostProcess(ProcessedImage image, ScanOptions options, PostProcessingContext postProcessingContext)
    {
        if (!string.IsNullOrEmpty(options.OcrParams.LanguageCode))
        {
            RunBackgroundOcr(ref image, options, postProcessingContext.TempPath);
        }
        return image;
    }

    private void RunBackgroundOcr(ref ProcessedImage image, ScanOptions options, string? tempPath)
    {
        if (tempPath == null)
        {
            throw new InvalidOperationException("Expected OCR tempPath to be set");
            // TODO: If we ever support a network scan bridge again, we'll want to set this here in that case
            // tempPath = _scanningContext.SaveToTempFile(image, options.BitDepth);
        }
        _ocrController.Start(ref image, tempPath, options.OcrParams, options.OcrPriority).AssertNoAwait();
    }
}