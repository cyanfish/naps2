using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;

namespace NAPS2.Images
{
    public class ScannedImageRenderer
    {
        public async Task<IImage> Render(ScannedImage image, int outputSize = 0)
        {
            using (var snapshot = image.Preserve())
            {
                return await Render(snapshot, outputSize);
            }
        }

        public async Task<IImage> Render(ScannedImage.Snapshot snapshot, int outputSize = 0)
        {
            return await Task.Factory.StartNew(() =>
            {
                var storage = StorageManager.ConvertToImage(snapshot.Source.BackingStorage, new StorageConvertParams());
                if (outputSize > 0)
                {
                    double scaleFactor = Math.Min(outputSize / (double)storage.Height, outputSize / (double)storage.Width);
                    storage = Transform.Perform(storage, new ScaleTransform { ScaleFactor = scaleFactor });
                }
                return Transform.PerformAll(storage, snapshot.TransformList);
            });
        }

        public async Task<Stream> RenderToStream(ScannedImage image)
        {
            using (var snapshot = image.Preserve())
            {
                return await RenderToStream(snapshot);
            }
        }

        public async Task<Stream> RenderToStream(ScannedImage.Snapshot snapshot)
        {
            using (var transformed = await Render(snapshot))
            {
                return StorageManager.Convert<MemoryStreamStorage>(transformed, new StorageConvertParams
                {
                    // TODO: Is this right?
                    Lossless = snapshot.Source.Metadata.Lossless
                }).Stream;
            }
        }
    }
}
