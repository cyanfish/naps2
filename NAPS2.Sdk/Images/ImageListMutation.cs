namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<UiImage>
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

        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            foreach (UiImage img in selection)
            {
                lock (img)
                {
                    var transform = new RotationTransform(_angle);
                    img.AddTransform(transform);
                    var thumb = img.GetThumbnailClone();
                    if (thumb != null)
                    {
                        img.SetThumbnail(_imageContext.PerformTransform(thumb, transform));
                    }
                }
            }
        }
    }

    public class ResetTransforms : ListMutation<UiImage>
    {
        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            foreach (UiImage img in selection)
            {
                img.ResetTransforms();
            }
        }
    }
}