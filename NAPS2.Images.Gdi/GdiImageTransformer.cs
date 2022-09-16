using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
public class GdiImageTransformer : AbstractImageTransformer<GdiImage>
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
        var resultImage = new GdiImage(ImageContext, result);
        OptimizePixelFormat(image, ref resultImage);
        image.Dispose();
        return resultImage;
    }

    protected override GdiImage PerformTransform(GdiImage image, ScaleTransform transform)
    {
        int realWidth = (int) Math.Round(image.Width * transform.ScaleFactor);
        int realHeight = (int) Math.Round(image.Height * transform.ScaleFactor);

        double horizontalRes = image.HorizontalResolution * transform.ScaleFactor;
        double verticalRes = image.VerticalResolution * transform.ScaleFactor;

        var result = new Bitmap(realWidth, realHeight, PixelFormat.Format24bppRgb);
        using Graphics g = Graphics.FromImage(result);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(image.Bitmap, 0, 0, realWidth, realHeight);
        result.SafeSetResolution((float) horizontalRes, (float) verticalRes);
        return new GdiImage(ImageContext, result);
    }

    protected override GdiImage PerformTransform(GdiImage image, ThumbnailTransform transform)
    {
        var result = new Bitmap(transform.Size, transform.Size);
        using (Graphics g = Graphics.FromImage(result))
        {
            // We want a nice thumbnail, so use the maximum quality interpolation
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Draw the original bitmap onto the new bitmap, using the calculated location and dimensions
            // Note that there may be some padding if the aspect ratios don't match
            var (left, top, width, height) = transform.GetDrawRect(image.Width, image.Height);
            var destRect = new RectangleF(left, top, width, height);
            var srcRect = new RectangleF(0, 0, image.Width, image.Height);
            g.DrawImage(image.Bitmap, destRect, srcRect, GraphicsUnit.Pixel);
            // Draw a border around the original bitmap's content, inside the padding
            g.DrawRectangle(Pens.Black, left, top, width - 1, height - 1);
        }
        return new GdiImage(ImageContext, result);
    }
}