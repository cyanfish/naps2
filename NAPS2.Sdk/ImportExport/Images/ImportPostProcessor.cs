using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImportPostProcessor
{
    private readonly ImageContext _imageContext;

    public ImportPostProcessor(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public ProcessedImage AddPostProcessingData(ProcessedImage image, IMemoryImage rendered, int? thumbnailSize, BarcodeDetectionOptions barcodeDetectionOptions, bool disposeProcessedImage)
    {
        // TODO: Ownership of the rendered image is messed up, need to figure out how to make it work with the transform
        // model and update callers
        var thumbnail = thumbnailSize.HasValue
            ? _imageContext.PerformTransform(rendered, new ThumbnailTransform(thumbnailSize.Value))
            : null;
        var barcodeDetection = BarcodeDetector.Detect(rendered, barcodeDetectionOptions);
        return image.WithPostProcessingData(image.PostProcessingData with
            {Thumbnail = thumbnail, BarcodeDetection = barcodeDetection});
    }
}