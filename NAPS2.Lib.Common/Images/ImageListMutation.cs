namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<UiImage>
{
    // TODO: Is there a better way to handle updating this thumbnail?
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
                var transform = new RotationTransform(_angle);
                var thumb = img.GetThumbnailClone();
                var updatedThumb = thumb != null ? _imageContext.PerformTransform(thumb, transform) : null;
                img.AddTransform(transform, updatedThumb);
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