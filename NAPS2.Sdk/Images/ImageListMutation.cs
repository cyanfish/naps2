namespace NAPS2.Images;

public abstract class ImageListMutation : ListMutation<RenderableImage>
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

        public override void Apply(List<RenderableImage> list, ref ListSelection<RenderableImage> selection)
        {
            foreach (RenderableImage img in selection)
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

    public class ResetTransforms : ListMutation<RenderableImage>
    {
        public override void Apply(List<RenderableImage> list, ref ListSelection<RenderableImage> selection)
        {
            foreach (RenderableImage img in selection)
            {
                // TODO: Propagate transforms
                // img.ResetTransforms();
            }
        }
    }
}