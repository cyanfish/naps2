using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class ThumbnailRenderer : IScannedImageRenderer<IImage>
    {
        private const int OVERSAMPLE = 3;
        
        private readonly ImageContext imageContext;
        private readonly IScannedImageRenderer<IImage> imageRenderer;

        public ThumbnailRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
            imageRenderer = new ImageRenderer(imageContext);
        }

        public ThumbnailRenderer(ImageContext imageContext, IScannedImageRenderer<IImage> imageRenderer)
        {
            this.imageContext = imageContext;
            this.imageRenderer = imageRenderer;
        }

        public async Task<IImage> Render(ScannedImage image, int outputSize)
        {
            using var snapshot = image.Preserve();
            return await Render(snapshot, outputSize);
        }

        public async Task<IImage> Render(ScannedImage.Snapshot snapshot, int outputSize)
        {
            using var bitmap = await imageRenderer.Render(snapshot, snapshot.Metadata.TransformList.Count == 0 ? 0 : outputSize * OVERSAMPLE);
            return imageContext.PerformTransform(bitmap, new ThumbnailTransform(outputSize));
        }
    }
}