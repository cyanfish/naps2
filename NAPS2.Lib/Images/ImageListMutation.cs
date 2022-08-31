namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<UiImage>
{
    public class RotateFlip : ImageListMutation
    {
        private readonly double _angle;

        public RotateFlip(double angle)
        {
            _angle = angle;
        }

        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            foreach (UiImage img in selection)
            {
                var transform = new RotationTransform(_angle);
                var thumb = img.GetThumbnailClone();
                var updatedThumb = thumb?.PerformTransform(transform);
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