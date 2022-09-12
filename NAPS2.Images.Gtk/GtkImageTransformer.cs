namespace NAPS2.Images.Gtk;

public class GtkImageTransformer : AbstractImageTransformer<GtkImage>
{
    public GtkImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override GtkImage PerformTransform(GtkImage image, RotationTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override GtkImage PerformTransform(GtkImage image, ScaleTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override GtkImage PerformTransform(GtkImage image, ThumbnailTransform transform)
    {
        throw new NotImplementedException();
    }
}