namespace NAPS2.Scan.Internal;

/// <summary>
/// Performs post-processing on an image after scanning. If a network-based IScanAdapter is used, this happens on the remote instance.
/// </summary>
internal interface IRemotePostProcessor
{
    RenderableImage PostProcess(IImage image, ScanOptions options, PostProcessingContext postProcessingContext);
}