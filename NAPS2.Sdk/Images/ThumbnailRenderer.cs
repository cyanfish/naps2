using NAPS2.Images.Gdi;

namespace NAPS2.Images;

public class ThumbnailRenderer
{
    private const int OVERSAMPLE = 3;
        
    private readonly ImageContext _imageContext;

    public ThumbnailRenderer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public IMemoryImage Render(ProcessedImage processedImage, int outputSize)
    {
        var image = _imageContext.Render(processedImage);
        var transformList = processedImage.TransformState.Transforms;
        if (!processedImage.TransformState.IsEmpty)
        {
            // When we have additional transformations, performing them on a large original image may be quite slow.
            // On the other hand, scaling the image to the thumbnail size first can result in transforms losing detail.
            // As a middle ground we scale to an "oversampled" size first.
            double oversampledSize = outputSize * OVERSAMPLE;
            double scaleFactor = Math.Min(oversampledSize / image.Height, oversampledSize / image.Width);
            transformList = transformList.Insert(0, new ScaleTransform(scaleFactor));
        }
        transformList = transformList.Add(new ThumbnailTransform(outputSize));
        return _imageContext.PerformAllTransforms(image, transformList);
    }
}