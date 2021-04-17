using NAPS2.Images;
using NAPS2.Ocr;
using NAPS2.Threading;

namespace NAPS2.Scan.Internal
{
    internal class LocalPostProcessor : ILocalPostProcessor
    {
        private readonly OcrRequestQueue _ocrRequestQueue;

        public LocalPostProcessor()
            : this(OcrRequestQueue.Default)
        {
        }

        public LocalPostProcessor(OcrRequestQueue ocrRequestQueue)
        {
            _ocrRequestQueue = ocrRequestQueue;
        }

        public void PostProcess(ScannedImage scannedImage, ScanOptions options, PostProcessingContext postProcessingContext)
        {
            if (postProcessingContext.TempPath != null)
            {
                RunBackgroundOcr(scannedImage, options, postProcessingContext.TempPath);
            }
        }

        private void RunBackgroundOcr(ScannedImage image, ScanOptions options, string tempPath)
        {
            if (options.DoOcr)
            {
                using var snapshot = image.Preserve();
                if (!options.OcrInBackground)
                {
                    _ocrRequestQueue.QueueForeground(null, snapshot, tempPath, options.OcrParams, options.OcrCancelToken).AssertNoAwait();
                }
                else
                {
                    _ocrRequestQueue.QueueBackground(snapshot, tempPath, options.OcrParams);
                }
            }
        }
    }
}