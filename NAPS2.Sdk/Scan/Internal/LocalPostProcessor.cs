using NAPS2.Ocr;

namespace NAPS2.Scan.Internal;

internal class LocalPostProcessor : ILocalPostProcessor
{
    private readonly OcrController _ocrController;

    public LocalPostProcessor(OcrController ocrController)
    {
        _ocrController = ocrController;
    }

    public ProcessedImage PostProcess(ProcessedImage image, ScanOptions options, PostProcessingContext postProcessingContext)
    {
        if (postProcessingContext.TempPath != null)
        {
            RunBackgroundOcr(ref image, options, postProcessingContext.TempPath);
        }
        return image;
    }

    private void RunBackgroundOcr(ref ProcessedImage image, ScanOptions options, string tempPath)
    { 
        _ocrController.Start(ref image, tempPath).AssertNoAwait();
    }
}