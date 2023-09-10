using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NAPS2.Images.Wpf;

public class WpfImageTransformer : AbstractImageTransformer<WpfImage>
{
    public WpfImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override WpfImage PerformTransform(WpfImage image, RotationTransform transform)
    {
        // TODO: We can maybe optimize this in some ways, e.g. skip a clone if we're already Rgba32, or convert the
        // final pixel format back to whatever the original was.

        var width = image.Width;
        var height = image.Height;
        float xres = image.HorizontalResolution, yres = image.VerticalResolution;
        if (transform.Angle is > 45.0 and < 135.0 or > 225.0 and < 315.0)
        {
            (width, height) = (height, width);
            (xres, yres) = (yres, xres);
        }

        var copy = new TransformedBitmap(image.Bitmap, new RotateTransform(transform.Angle));

        var newImage = new WpfImage(ImageContext, new WriteableBitmap(copy));
        // TODO: In Gdi, we convert this back to BW1. Should we do the same?
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(xres, yres);
        image.Dispose();
        return newImage;
    }

    protected override WpfImage PerformTransform(WpfImage image, ResizeTransform transform)
    {
        var copy = new TransformedBitmap(image.Bitmap,
            new System.Windows.Media.ScaleTransform(transform.Width / (double) image.Width,
                transform.Height / (double) image.Height));
        var newImage = new WpfImage(ImageContext, new WriteableBitmap(copy));
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        image.Dispose();
        return newImage;
    }
}