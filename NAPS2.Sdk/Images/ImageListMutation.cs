using System.Collections.Generic;
using NAPS2.Images.Storage;
using NAPS2.Images.Transforms;
using NAPS2.Util;

namespace NAPS2.Images
{
    public abstract class ImageListMutation : ListMutation<ScannedImage>
    {
        public class DeleteAll : ImageListMutation
        {
            public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
            {
                foreach (var image in list)
                {
                    image.Dispose();
                }
                list.Clear();
            }
        }

        public class DeleteSelected : ImageListMutation
        {
            public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
            {
                foreach (var image in selection)
                {
                    image.Dispose();
                }
                list.RemoveAll(selection);
            }
        }

        public class RotateFlip : ImageListMutation
        {
            private readonly ImageContext imageContext;
            private readonly double angle;

            public RotateFlip(ImageContext imageContext, double angle)
            {
                this.imageContext = imageContext;
                this.angle = angle;
            }

            public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
            {
                foreach (ScannedImage img in selection)
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
            }
        }

        public class ResetTransforms : ListMutation<ScannedImage>
        {
            public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
            {
                foreach (ScannedImage img in selection)
                {
                    img.ResetTransforms();
                }
            }
        }

        public class InsertAt : ListMutation<ScannedImage>
        {
            private readonly int index;
            private readonly ScannedImage scannedImage;

            public InsertAt(int index, ScannedImage scannedImage)
            {
                this.index = index;
                this.scannedImage = scannedImage;
            }

            public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
            {
                list.Insert(index, scannedImage);
            }
        }
    }
}
