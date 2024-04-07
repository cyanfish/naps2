namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<UiImage>
{
    public class RotateFlip(double angle) : ImageListMutation
    {
        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            foreach (UiImage img in selection)
            {
                var transform = new RotationTransform(angle);
                var thumb = img.GetThumbnailClone();
                var updatedThumb = thumb?.PerformTransform(transform);
                img.AddTransform(transform, updatedThumb);
            }
        }
    }

    public class AltFlip() : ImageListMutation
    {
        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            bool toggle = false;
            foreach (UiImage img in selection)
            {
                if (toggle)
                {
                    var transform = new RotationTransform(180);
                    var thumb = img.GetThumbnailClone();
                    var updatedThumb = thumb?.PerformTransform(transform);
                    img.AddTransform(transform, updatedThumb);
                }
                toggle = !toggle;
            }
        }
    }

    public class AddTransforms(List<Transform> transforms, Dictionary<UiImage, IMemoryImage?>? updatedThumbnails = null)
        : ListMutation<UiImage>
    {
        public override void Apply(List<UiImage> list, ref ListSelection<UiImage> selection)
        {
            if (transforms.Any(x => !x.IsNull))
            {
                foreach (UiImage img in selection)
                {
                    img.AddTransforms(transforms, updatedThumbnails?.Get(img));
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