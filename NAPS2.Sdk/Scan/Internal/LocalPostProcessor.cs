using NAPS2.Images;
using NAPS2.Ocr;
using NAPS2.Util;

namespace NAPS2.Scan.Internal
{
    internal class LocalPostProcessor : ILocalPostProcessor
    {
        private readonly OcrRequestQueue ocrRequestQueue;

        public LocalPostProcessor()
            : this(OcrRequestQueue.Default)
        {
        }

        public LocalPostProcessor(OcrRequestQueue ocrRequestQueue)
        {
            this.ocrRequestQueue = ocrRequestQueue;
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
                using (var snapshot = image.Preserve())
                {
                    if (!options.OcrInBackground)
                    {
                        ocrRequestQueue.QueueForeground(null, snapshot, tempPath, options.OcrParams, options.OcrCancelToken).AssertNoAwait();
                    }
                    else
                    {
                        ocrRequestQueue.QueueBackground(snapshot, tempPath, options.OcrParams);
                    }
                }
            }
        }
    }
}