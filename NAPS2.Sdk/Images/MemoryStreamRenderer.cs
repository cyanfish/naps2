using System.IO;
using System.Threading.Tasks;
using NAPS2.Images.Storage;

namespace NAPS2.Images
{
    public class MemoryStreamRenderer : IScannedImageRenderer<MemoryStream>
    {
        private readonly ImageContext imageContext;
        private readonly IScannedImageRenderer<IImage> imageRenderer;

        public MemoryStreamRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
            imageRenderer = new ImageRenderer(imageContext);
        }

        public MemoryStreamRenderer(IScannedImageRenderer<IImage> imageRenderer)
        {
            this.imageRenderer = imageRenderer;
        }

        public async Task<MemoryStream> Render(ScannedImage image, int outputSize = 0)
        {
            using var snapshot = image.Preserve();
            return await Render(snapshot, outputSize);
        }

        public async Task<MemoryStream> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            using var transformed = await imageRenderer.Render(snapshot);
            return imageContext.Convert<MemoryStreamStorage>(transformed, new StorageConvertParams
            {
                // TODO: Is this right?
                Lossless = snapshot.Source.Metadata.Lossless
            }).Stream;
        }
    }
}
