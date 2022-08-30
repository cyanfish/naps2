using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

public class ImportPostProcessor
{
    private readonly ImageContext _imageContext;

    public ImportPostProcessor(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public ProcessedImage AddPostProcessingData(ProcessedImage image, IMemoryImage? rendered, int? thumbnailSize,
        BarcodeDetectionOptions barcodeDetectionOptions, bool disposeOriginalImage)
    {
        if (!thumbnailSize.HasValue && !barcodeDetectionOptions.DetectBarcodes)
        {
            // This is a bit weird, but technically "disposeOriginalImage" doesn't mean we're actually disposing it,
            // just that the caller releases ownership of it (and takes ownership of the return value).
            return disposeOriginalImage ? image : image.Clone();
        }

        var disposeRendered = rendered == null;
        rendered ??= image.Render();
        try
        {
            var thumbnail = thumbnailSize.HasValue
                ? _imageContext.PerformTransform(rendered, new ThumbnailTransform(thumbnailSize.Value))
                : null;
            var barcodeDetection = BarcodeDetector.Detect(rendered, barcodeDetectionOptions);
            return image.WithPostProcessingData(image.PostProcessingData with
            {
                Thumbnail = thumbnail,
                ThumbnailTransformState = image.TransformState,
                BarcodeDetection = barcodeDetection
            }, disposeOriginalImage);
        }
        finally
        {
            if (disposeRendered)
            {
                rendered.Dispose();
            }
        }
    }
}