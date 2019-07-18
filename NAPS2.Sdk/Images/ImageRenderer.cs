using System;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class ImageRenderer : IScannedImageRenderer<IImage>
    {
        private readonly ImageContext imageContext;

        public ImageRenderer(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        public async Task<IImage> Render(ScannedImage image, int outputSize = 0)
        {
            using var snapshot = image.Preserve();
            return await Render(snapshot, outputSize);
        }

        public async Task<IImage> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return await Task.Run(() =>
            {
                var storage = imageContext.ConvertToImage(snapshot.Source.BackingStorage, new StorageConvertParams());
                if (outputSize > 0)
                {
                    double scaleFactor = Math.Min(outputSize / (double)storage.Height, outputSize / (double)storage.Width);
                    storage = imageContext.PerformTransform(storage, new ScaleTransform(scaleFactor));
                }
                return imageContext.PerformAllTransforms(storage, snapshot.Metadata.TransformList);
            });
        }
    }
}
