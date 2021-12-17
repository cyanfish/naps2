namespace NAPS2.Images;

public class ThumbnailRenderer : IScannedImageRenderer<IImage>
{
    private const int OVERSAMPLE = 3;
        
    private readonly ImageContext _imageContext;
    private readonly IScannedImageRenderer<IImage> _imageRenderer;

    public ThumbnailRenderer(ImageContext imageContext)
    {
        _imageContext = imageContext;
        _imageRenderer = new ImageRenderer(imageContext);
    }

    public ThumbnailRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
    {
        _imageContext = imageContext;
        _imageRenderer = imageRenderer;
    }

    public async Task<IImage> Render(ScannedImage image, int outputSize)
    {
        using var snapshot = image.Preserve();
        return await Render(snapshot, outputSize);
    }

    public async Task<IImage> Render(ScannedImage.Snapshot snapshot, int outputSize)
    {
        using var bitmap = await _imageRenderer.Render(snapshot, snapshot.Metadata.TransformList.Count == 0 ? 0 : outputSize * OVERSAMPLE);
        return _imageContext.PerformTransform(bitmap, new ThumbnailTransform(outputSize));
    }
}