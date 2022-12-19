namespace NAPS2.Images.Mac;

public class MacImageTransformer : AbstractImageTransformer<MacImage>
{
    public MacImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override MacImage PerformTransform(MacImage image, RotationTransform transform)
    {
        MacImage newImage;
        var pixelFormat = GetDrawingPixelFormat(image);
        if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
        {
            newImage = (MacImage) ImageContext.Create(image.Height, image.Width, pixelFormat);
            newImage.SetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        else
        {
            newImage = (MacImage) image.CopyBlankWithPixelFormat(pixelFormat);
        }
        using CGBitmapContext c = MacBitmapHelper.CreateContext(newImage);

        CGRect fillRect = new CGRect(0, 0, newImage.Width, newImage.Height);
        c.SetFillColor(new CGColor(255.ToNFloat(), 255.ToNFloat(), 255.ToNFloat(), 255.ToNFloat()));
        c.FillRect(fillRect);

        var t1 = CGAffineTransform.MakeTranslation((-image.Width / 2.0).ToNDouble(), (-image.Height / 2.0).ToNDouble());
        var t2 = CGAffineTransform.MakeRotation((-transform.Angle * Math.PI / 180).ToNDouble());
        var t3 = CGAffineTransform.MakeTranslation((newImage.Width / 2.0).ToNDouble(),
            (newImage.Height / 2.0).ToNDouble());
        c.ConcatCTM(CGAffineTransform.Multiply(CGAffineTransform.Multiply(t1, t2), t3));

        CGRect rect = new CGRect(0, 0, image.Width, image.Height);
        c.DrawImage(rect, image.Rep.AsCGImage(ref rect, null, null));
        image.Dispose();
        return newImage;
    }

    protected override MacImage PerformTransform(MacImage image, ResizeTransform transform)
    {
        var newImage = (MacImage) ImageContext.Create(transform.Width, transform.Height, GetDrawingPixelFormat(image));
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        using CGBitmapContext c = MacBitmapHelper.CreateContext(newImage);
        CGRect rect = new CGRect(0, 0, transform.Width, transform.Height);
        // TODO: This changes the image size to match the original which we probably don't want.
        c.DrawImage(rect, image.Rep.AsCGImage(ref rect, null, null));
        image.Dispose();
        return newImage;
    }

    private static ImagePixelFormat GetDrawingPixelFormat(MacImage image)
    {
        return image.PixelFormat switch
        {
            ImagePixelFormat.BW1 or ImagePixelFormat.Gray8 => ImagePixelFormat.Gray8,
            ImagePixelFormat.RGB24 => ImagePixelFormat.RGB24,
            ImagePixelFormat.ARGB32 => ImagePixelFormat.ARGB32,
            _ => throw new ArgumentException("Unsupported pixel format")
        };
    }
}