using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImportPostProcessor
{
    private readonly ImageContext _imageContext;

    public ImportPostProcessor(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public ProcessedImage AddPostProcessingData(ProcessedImage image, IMemoryImage rendered, int? thumbnailSize,
        BarcodeDetectionOptions barcodeDetectionOptions, bool disposeOriginalImage)
    {
        var thumbnail = thumbnailSize.HasValue
            ? _imageContext.PerformTransform(rendered, new ThumbnailTransform(thumbnailSize.Value))
            : null;
        var barcodeDetection = BarcodeDetector.Detect(rendered, barcodeDetectionOptions);
        return image.WithPostProcessingData(image.PostProcessingData with
        {
            Thumbnail = thumbnail,
            BarcodeDetection = barcodeDetection
        }, disposeOriginalImage);
    }
}