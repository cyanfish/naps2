using NAPS2.Images;

namespace NAPS2.Scan.Internal
{
    /// <summary>
    /// Performs local post-processing on an image just before it is returned from IScanController.
    /// </summary>
    internal interface ILocalPostProcessor
    {
        void PostProcess(ScannedImage image, ScanOptions options, PostProcessingContext postProcessingContext);
    }
}
