using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
internal class GdiImageTransformer : AbstractImageTransformer<GdiImage>
{
    public GdiImageTransformer(ImageContext imageContext) : base(imageContext)
    {
    }

    protected override GdiImage PerformTransform(GdiImage image, RotationTransform transform)
    {
        if (Math.Abs(transform.Angle - 0.0) < RotationTransform.TOLERANCE)
        {
            return image;
        }
        if (Math.Abs(transform.Angle - 90.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return image;
        }
        if (Math.Abs(transform.Angle - 180.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
            return image;
        }
        if (Math.Abs(transform.Angle - 270.0) < RotationTransform.TOLERANCE)
        {
            image.Bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
            return image;
        }
        Bitmap result;
        if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
        {
            result = new Bitmap(image.Height, image.Width, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        else
        {
            result = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            result.SafeSetResolution(image.HorizontalResolution, image.VerticalResolution);
        }
        using (var g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);
            g.TranslateTransform(result.Width / 2.0f, result.Height / 2.0f);
            g.RotateTransform((float) transform.Angle);
            g.TranslateTransform(-image.Width / 2.0f, -image.Height / 2.0f);
            g.DrawImage(image.Bitmap, new Rectangle(0, 0, image.Width, image.Height));
        }
        var resultImage = new GdiImage(result);
        OptimizePixelFormat(image, ref resultImage);
        image.Dispose();
        return resultImage;
    }

    protected override GdiImage PerformTransform(GdiImage image, ResizeTransform transform)
    {
        var result = new Bitmap(transform.Width, transform.Height,
            image.PixelFormat == ImagePixelFormat.ARGB32
                ? PixelFormat.Format32bppArgb
                : PixelFormat.Format24bppRgb);
        using Graphics g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        // We set WrapMode to avoid artifacts
        // https://stackoverflow.com/questions/4772273/interpolationmode-highqualitybicubic-introducing-artefacts-on-edge-of-resized-im
        using var imageAttrs = new ImageAttributes();
        imageAttrs.SetWrapMode(WrapMode.TileFlipXY);
        var destRect = new Rectangle(0, 0, transform.Width, transform.Height);
        var srcW = image.Width;
        var srcH = image.Height;
        if (transform.Width < image.Width && transform.Height < image.Height)
        {
            // For some reason we have to use +1 to get it to align correctly
            srcW++;
            srcH++;
        }
        g.DrawImage(image.Bitmap, destRect, 0, 0, srcW, srcH, GraphicsUnit.Pixel, imageAttrs);
        result.SetResolution(
            image.HorizontalResolution * image.Width / transform.Width,
            image.VerticalResolution * image.Height / transform.Height);
        image.Dispose();
        return new GdiImage(result);
    }
}