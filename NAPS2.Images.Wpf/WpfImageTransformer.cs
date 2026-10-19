using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NAPS2.Images.Wpf;

internal class WpfImageTransformer : AbstractImageTransformer<WpfImage>
{
    public WpfImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override WpfImage PerformTransform(WpfImage image, RotationTransform transform)
    {
        if (Math.Abs(transform.Angle - 0.0) < RotationTransform.TOLERANCE)
        {
            return image;
        }
        if (Math.Abs(transform.Angle - 90.0) < RotationTransform.TOLERANCE)
        {
            return RotateRightAngle(image, 90);
        }
        if (Math.Abs(transform.Angle - 180.0) < RotationTransform.TOLERANCE)
        {
            return RotateRightAngle(image, 180);
        }
        if (Math.Abs(transform.Angle - 270.0) < RotationTransform.TOLERANCE)
        {
            return RotateRightAngle(image, 270);
        }

        var width = image.Width;
        var height = image.Height;
        float xres = image.HorizontalResolution, yres = image.VerticalResolution;
        if (transform.Angle is > 45.0 and < 135.0 or > 225.0 and < 315.0)
        {
            (width, height) = (height, width);
            (xres, yres) = (yres, xres);
        }

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            dc.PushTransform(new TranslateTransform(width / 2.0, height / 2.0));
            dc.PushTransform(new RotateTransform(transform.Angle));
            dc.PushTransform(new TranslateTransform(-image.Width / 2.0, -image.Height / 2.0));
            dc.DrawImage(image.ToBitmapSource(), new Rect(0, 0, image.Width, image.Height));
        }

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(visual);

        // The result is always opaque (with a white background), so we can use 24-bit unconditionally
        var newImage = new WpfImage(new FormatConvertedBitmap(rtb, PixelFormats.Bgr24, null, 0));
        // TODO: In Gdi, we convert this back to BW1 (or the original pixel format). Should we do the same?
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(xres, yres);
        image.Dispose();
        return newImage;
    }

    private static WpfImage RotateRightAngle(WpfImage image, int angle)
    {
        var rotated = new TransformedBitmap(image.ToBitmapSource(), new RotateTransform(angle));
        var newImage = new WpfImage(rotated)
        {
            LogicalPixelFormat = image.LogicalPixelFormat
        };
        if (angle == 180)
        {
            newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        }
        else
        {
            newImage.SetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        image.Dispose();
        return newImage;
    }

    protected override WpfImage PerformTransform(WpfImage image, ResizeTransform transform)
    {
        var copy = new TransformedBitmap(image.ToBitmapSource(),
            new System.Windows.Media.ScaleTransform(transform.Width / (double) image.Width,
                transform.Height / (double) image.Height));
        var newImage = new WpfImage(copy);
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