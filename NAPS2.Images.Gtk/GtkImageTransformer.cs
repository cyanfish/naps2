using Cairo;
using Gdk;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gtk;

public class GtkImageTransformer : AbstractImageTransformer<GtkImage>
{
    public GtkImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override GtkImage PerformTransform(GtkImage image, RotationTransform transform)
    {
        var format = image.PixelFormat == ImagePixelFormat.ARGB32 ? Format.Argb32 : Format.Rgb24;
        int width = image.Width, height = image.Height;
        float xres = image.HorizontalResolution, yres = image.VerticalResolution;
        if (transform.Angle is > 45.0 and < 135.0 or > 225.0 and < 315.0)
        {
            (width, height) = (height, width);
            (xres, yres) = (yres, xres);
        }

        using var surface = new ImageSurface(format, width, height);
        using var context = new Context(surface);
        context.Fill();
        context.SetSourceColor(new Cairo.Color(1, 1, 1));
        context.Paint();
        context.Translate(width / 2.0, height / 2.0);
        context.Rotate(transform.Angle * Math.PI / 180);
        context.Translate(-image.Width / 2.0, -image.Height / 2.0);
        CairoHelper.SetSourcePixbuf(context, image.Pixbuf, 0, 0);
        context.Paint();
        var newImage = new GtkImage(ImageContext, new Pixbuf(surface, 0, 0, width, height));
        // TODO: In Gdi, we convert this back to BW1. Should we do the same?
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(xres, yres);
        image.Dispose();
        return newImage;
    }

    protected override GtkImage PerformTransform(GtkImage image, ResizeTransform transform)
    {
        // TODO: Can we improve interpolation? Somehow integrate Cairo.Filter.Bilinear or Cairo.Filter.Best, though
        // it's not clear how to reconcile that with SetSourcePixbuf.
        var format = image.PixelFormat == ImagePixelFormat.ARGB32 ? Format.Argb32 : Format.Rgb24;
        using var surface = new ImageSurface(format, transform.Width, transform.Height);
        using var context = new Context(surface);
        context.Scale(transform.Width / (double) image.Width, transform.Height / (double) image.Height);
        CairoHelper.SetSourcePixbuf(context, image.Pixbuf, 0, 0);
        context.Paint();
        var newImage = new GtkImage(ImageContext, new Pixbuf(surface, 0, 0, transform.Width, transform.Height));
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        image.Dispose();
        return newImage;
    }

    protected override GtkImage PerformTransform(GtkImage image, BlackWhiteTransform transform)
    {
        new DecolorBitwiseImageOp(true)
        {
            BlackWhiteThreshold = (transform.Threshold + 1000) / 2000f
        }.Perform(image);
        image.LogicalPixelFormat = ImagePixelFormat.BW1;
        return image;
    }
}