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
            if (string.IsNullOrEmpty(options.NetworkOptions.Ip) || options.NetworkOptions.Port == null)
            {
                throw new InvalidOperationException("Expected OCR tempPath to be set for non-network scan");
            }
            // TODO: Make sure we have test coverage for both remote and local save cases
            var path = Path.Combine(_scanningContext.TempFolderPath, Path.GetRandomFileName());
            using var rendered = _scanningContext.ImageContext.Render(image);
            tempPath = _scanningContext.ImageContext.SaveSmallestFormat(rendered, path, options.BitDepth, false, -1, out _);
        }
        _ocrController.Start(ref image, tempPath, options.OcrParams, options.OcrPriority).AssertNoAwait();
    }
}