using MonoMac.CoreGraphics;

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
        MacImage newImage;
        if (transform.Angle > 45.0 && transform.Angle < 135.0 || transform.Angle > 225.0 && transform.Angle < 315.0)
        {
            newImage = (MacImage) ImageContext.Create(image.Height, image.Width, ImagePixelFormat.ARGB32);
            newImage.SetResolution(image.VerticalResolution, image.HorizontalResolution);
        }
        else
        {
            newImage = (MacImage) ImageContext.Create(image.Width, image.Height, ImagePixelFormat.ARGB32);
            newImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);
        }
        using CGBitmapContext c = GetCgBitmapContext(newImage);

        CGRect fillRect = new CGRect(0, 0, newImage.Width, newImage.Height);
        c.SetFillColor(new CGColor(255, 255, 255, 255));
        c.FillRect(fillRect);

        var t1 = CGAffineTransform.MakeTranslation(-image.Width / 2.0, -image.Height / 2.0);
        var t2 = CGAffineTransform.MakeRotation(-transform.Angle * Math.PI / 180);
        var t3 = CGAffineTransform.MakeTranslation(newImage.Width / 2.0, newImage.Height / 2.0);
        c.ConcatCTM(CGAffineTransform.Multiply(CGAffineTransform.Multiply(t1, t2), t3));

        CGRect rect = new CGRect(0, 0, image.Width, image.Height);
        c.DrawImage(rect, image._imageRep.AsCGImage(ref rect, null, null));
        return newImage;
    }

    // TODO: Add tests
    protected override MacImage PerformTransform(MacImage image, ScaleTransform transform)
    {
        var width = (int) Math.Round(image.Width * transform.ScaleFactor);
        var height = (int) Math.Round(image.Height * transform.ScaleFactor);
        var pixelFormat = image.PixelFormat switch
        {
            ImagePixelFormat.BW1 or ImagePixelFormat.Gray8 => ImagePixelFormat.Gray8,
            ImagePixelFormat.RGB24 => ImagePixelFormat.RGB24,
            ImagePixelFormat.ARGB32 => ImagePixelFormat.ARGB32,
            _ => throw new ArgumentException("Unsupported pixel format")
        };
        var newImage = (MacImage) ImageContext.Create(width, height, pixelFormat);
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / width,
            image.VerticalResolution * image.Height / height);
        using CGBitmapContext c = GetCgBitmapContext(newImage);
        CGRect rect = new CGRect(0, 0, width, height);
        c.DrawImage(rect, image._imageRep.AsCGImage(ref rect, null, null));
        return newImage;
    }

    private static CGBitmapContext GetCgBitmapContext(MacImage image)
    {
        lock (MacImageContext.ConstructorLock)
        {
            return new CGBitmapContext(
                image._imageRep.BitmapData,
                image.Width,
                image.Height,
                (int) image._imageRep.BitsPerSample,
                (int) image._imageRep.BytesPerRow,
                CGColorSpace.CreateDeviceRGB(),
                CGImageAlphaInfo.PremultipliedLast);
        }
    }

    protected override MacImage PerformTransform(MacImage image, ThumbnailTransform transform)
    {
        var pixelFormat = image.PixelFormat switch
        {
            ImagePixelFormat.BW1 or ImagePixelFormat.Gray8 => ImagePixelFormat.Gray8,
            ImagePixelFormat.RGB24 => ImagePixelFormat.RGB24,
            ImagePixelFormat.ARGB32 => ImagePixelFormat.ARGB32,
            _ => throw new ArgumentException("Unsupported pixel format")
        };
        var newImage = (MacImage) ImageContext.Create(transform.Size, transform.Size, pixelFormat);
        var (left, top, width, height) = transform.GetDrawRect(image.Width, image.Height);
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / width,
            image.VerticalResolution * image.Height / height);
        using CGBitmapContext c = GetCgBitmapContext(newImage);
        CGRect rect = new CGRect(left, top, width, height);
        c.DrawImage(rect, image._imageRep.AsCGImage(ref rect, null, null));

        CGRect strokeRect = new CGRect(left + 0.5, top + 0.5, width - 1, height - 1);
        c.SetRGBStrokeColor(0, 0, 0, 255);
        c.StrokeRect(strokeRect);

        return newImage;
    }
}