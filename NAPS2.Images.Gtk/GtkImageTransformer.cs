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
        return newImage;
    }

    protected override GtkImage PerformTransform(GtkImage image, ScaleTransform transform)
    {
        var format = image.PixelFormat == ImagePixelFormat.ARGB32 ? Format.Argb32 : Format.Rgb24;
        int width = (int) Math.Round(image.Width * transform.ScaleFactor);
        int height = (int) Math.Round(image.Height * transform.ScaleFactor);
        using var surface = new ImageSurface(format, width, height);
        using var context = new Context(surface);
        context.Scale(width / (double) image.Width, height / (double) image.Height);
        CairoHelper.SetSourcePixbuf(context, image.Pixbuf, 0, 0);
        context.Paint();
        var newImage = new GtkImage(ImageContext, new Pixbuf(surface, 0, 0, width, height));
        newImage.LogicalPixelFormat = image.LogicalPixelFormat == ImagePixelFormat.BW1
            ? ImagePixelFormat.Gray8
            : image.LogicalPixelFormat;
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / width,
            image.VerticalResolution * image.Height / height);
        return newImage;
    }

    protected override GtkImage PerformTransform(GtkImage image, ThumbnailTransform transform)
    {
        var (_, _, width, height) = transform.GetDrawRect(image.Width, image.Height);
        var scaleFactor = width > height ? width / (double) image.Width : height / (double) image.Height;
        return PerformTransform(image, new ScaleTransform(scaleFactor));
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