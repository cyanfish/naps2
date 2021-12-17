namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<ScannedImage>
{
    public class RotateFlip : ImageListMutation
    {
        private readonly ImageContext _imageContext;
        private readonly double _angle;

        public RotateFlip(ImageContext imageContext, double angle)
        {
            _imageContext = imageContext;
            _angle = angle;
        }

        public override void Apply(List<ScannedImage> list, ref ListSelection<ScannedImage> selection)
        {
            foreach (ScannedImage img in selection)
            {
                lock (img)
                {
                    var transform = new RotationTransform(_angle);
                    img.AddTransform(transform);
                    var thumb = img.GetThumbnail();
                    if (thumb != null)
                    {
                        img.SetThumbnail(_imageContext.PerformTransform(thumb, transform));
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
}