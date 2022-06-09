namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<ProcessedImage>
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

        public override void Apply(List<ProcessedImage> list, ref ListSelection<ProcessedImage> selection)
        {
            foreach (ProcessedImage img in selection)
            {
                lock (img)
                {
                    var transform = new RotationTransform(_angle);
                    // TODO: Propagate transforms
                    // img.AddTransform(transform);
                    // var thumb = img.GetThumbnail();
                    // if (thumb != null)
                    // {
                    //     img.SetThumbnail(_imageContext.PerformTransform(thumb, transform));
                    // }
                }
            }
        }
    }

    public class ResetTransforms : ListMutation<ProcessedImage>
    {
        public override void Apply(List<ProcessedImage> list, ref ListSelection<ProcessedImage> selection)
        {
            foreach (ProcessedImage img in selection)
            {
                // TODO: Propagate transforms
                // img.ResetTransforms();
            }
        }
    }
}