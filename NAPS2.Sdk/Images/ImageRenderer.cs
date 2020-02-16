using System;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class ImageRenderer : IScannedImageRenderer<IImage>
    {
        private readonly ImageContext _imageContext;

        public ImageRenderer(ImageContext imageContext)
        {
            _imageContext = imageContext;
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
                var storage = _imageContext.ConvertToImage(snapshot.Source.BackingStorage, new StorageConvertParams());
                if (outputSize > 0)
                {
                    double scaleFactor = Math.Min(outputSize / (double)storage.Height, outputSize / (double)storage.Width);
                    storage = _imageContext.PerformTransform(storage, new ScaleTransform(scaleFactor));
                }
                return _imageContext.PerformAllTransforms(storage, snapshot.Metadata.TransformList);
            });
        }
    }
}
