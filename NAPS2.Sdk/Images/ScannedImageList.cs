using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Images
{
    public class ScannedImageList
    {
        private readonly ImageContext imageContext;

        public ScannedImageList(ImageContext imageContext)
        {
            this.imageContext = imageContext;
            Images = new List<ScannedImage>();
        }

        public ScannedImageList(ImageContext imageContext, List<ScannedImage> images)
        {
            this.imageContext = imageContext;
            Images = images;
        }

        public ThumbnailRenderer ThumbnailRenderer { get; set; }

        public List<ScannedImage> Images { get; }

        public async Task RotateFlip(IEnumerable<int> selection, double angle)
        {
            var images = Images.ElementsAt(selection).ToList();
            await Task.Run(() =>
            {
                foreach (ScannedImage img in images)
                {
                    lock (img)
                    {
                        var transform = new RotationTransform(angle);
                        img.AddTransform(transform);
                        var thumb = img.GetThumbnail();
                        if (thumb != null)
                        {
                            img.SetThumbnail(imageContext.PerformTransform(thumb, transform));
                        }
                    }
                }
            });
        }

        public void ResetTransforms(IEnumerable<int> selection)
        {
            foreach (ScannedImage img in Images.ElementsAt(selection))
            {
                img.ResetTransforms();
            }
        }
    }
}
