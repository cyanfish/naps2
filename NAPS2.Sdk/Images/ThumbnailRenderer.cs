namespace NAPS2.Images;

// TODO: Use this in more places, i.e. ImportPostProcessor
/// <summary>
/// Renders a ProcessedImage to a thumbnail of a given size.
/// </summary>
public class ThumbnailRenderer
{
    private const int OVERSAMPLE = 3;

    private readonly ImageContext _imageContext;

    public ThumbnailRenderer(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public Task<IMemoryImage> Render(ProcessedImage processedImage, int outputSize)
    {
        // On Mac it's really important this runs as a task, as Mac memory management depends on autorelease pools which
        // in Xamarin operate at the task level. Otherwise a long-running thumbnail render thread will leak memory like
        // a sieve.
        return Task.Run(() =>
        {
            var image = _imageContext.RenderWithoutTransforms(processedImage);
            var transformList = processedImage.TransformState.Transforms;
            if (!processedImage.TransformState.IsEmpty)
            {
                // When we have additional transformations, performing them on a large original image may be quite slow.
                // On the other hand, scaling the image to the thumbnail size first can result in transforms losing detail.
                // As a middle ground we scale to an "oversampled" size first.
                double oversampledSize = outputSize * OVERSAMPLE;
                double scaleFactor = Math.Min(oversampledSize / image.Height, oversampledSize / image.Width);
                scaleFactor = Math.Min(scaleFactor, 1);
                transformList = transformList.Insert(0, new ScaleTransform(scaleFactor));
            }
            transformList = transformList.Add(new ThumbnailTransform(outputSize));
            return _imageContext.PerformAllTransforms(image, transformList);
        });
    }
}