using System.IO;
using System.Threading.Tasks;
using NAPS2.Images.Storage;

namespace NAPS2.Images;

public class MemoryStreamRenderer : IScannedImageRenderer<MemoryStream>
{
    private readonly ImageContext _imageContext;
    private readonly IScannedImageRenderer<IImage> _imageRenderer;

    public MemoryStreamRenderer(ImageContext imageContext)
    {
        _imageContext = imageContext;
        _imageRenderer = new ImageRenderer(imageContext);
    }

    public MemoryStreamRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
    {
        _imageContext = imageContext;
        _imageRenderer = imageRenderer;
    }

    public async Task<MemoryStream> Render(ScannedImage image, int outputSize = 0)
    {
        using var snapshot = image.Preserve();
        return await Render(snapshot, outputSize);
    }

    public async Task<MemoryStream> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
    {
        using var transformed = await _imageRenderer.Render(snapshot);
        return _imageContext.Convert<MemoryStreamStorage>(transformed, new StorageConvertParams
        {
            // TODO: Is this right?
            Lossless = snapshot.Source.Metadata.Lossless
        }).Stream;
    }
}