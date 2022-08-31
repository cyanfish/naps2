using NAPS2.Scan;

namespace NAPS2.ImportExport.Images;

// TODO: Maybe make this static (and ImportExportHelper, ImageClipboard, etc.) now that ImageContext is on the image
public class ImportPostProcessor
{
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
                ? rendered.PerformTransform(new ThumbnailTransform(thumbnailSize.Value))
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