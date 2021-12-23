using NAPS2.Ocr;

namespace NAPS2.Scan.Internal;

internal class LocalPostProcessor : ILocalPostProcessor
{
    private readonly ScanningContext _scanningContext;

    public LocalPostProcessor(ScanningContext scanningContext)
    {
        _scanningContext = scanningContext;
    }

    public void PostProcess(RenderableImage image, ScanOptions options, PostProcessingContext postProcessingContext)
    {
        if (postProcessingContext.TempPath != null)
        {
            RunBackgroundOcr(image, options, postProcessingContext.TempPath);
        }
    }

    private void RunBackgroundOcr(RenderableImage image, ScanOptions options, string tempPath)
    {
        if (options.DoOcr)
        {
            if (!options.OcrInBackground)
            {
                _scanningContext.OcrRequestQueue.QueueForeground(null, image, tempPath, options.OcrParams, options.OcrCancelToken).AssertNoAwait();
            }
            else
            {
                _scanningContext.OcrRequestQueue.QueueBackground(image, tempPath, options.OcrParams);
            }
        }
    }
}