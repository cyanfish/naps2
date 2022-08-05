namespace NAPS2.Images.Mac;

public class MacImageTransformer : AbstractImageTransformer<MacImage>
{
    public MacImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override MacImage PerformTransform(MacImage image, ContrastTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, SaturationTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, SharpenTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, RotationTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, CropTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, ScaleTransform transform)
    {
        throw new NotImplementedException();
    }

    protected override MacImage PerformTransform(MacImage image, ThumbnailTransform transform)
    {
        throw new NotImplementedException();
    }
}