namespace NAPS2.Images.Mac;

public class MacImageTransformer : AbstractImageTransformer<MacImage>
{
    public MacImageTransformer(ImageContext imageContext) : base(imageContext)
    {
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
        c.SetFillColor(new CGColor(255.ToNFloat(), 255.ToNFloat(), 255.ToNFloat(), 255.ToNFloat()));
        c.FillRect(fillRect);

        var t1 = CGAffineTransform.MakeTranslation((-image.Width / 2.0).ToNDouble(), (-image.Height / 2.0).ToNDouble());
        var t2 = CGAffineTransform.MakeRotation((-transform.Angle * Math.PI / 180).ToNDouble());
        var t3 = CGAffineTransform.MakeTranslation((newImage.Width / 2.0).ToNDouble(),
            (newImage.Height / 2.0).ToNDouble());
        c.ConcatCTM(CGAffineTransform.Multiply(CGAffineTransform.Multiply(t1, t2), t3));

        CGRect rect = new CGRect(0, 0, image.Width, image.Height);
        c.DrawImage(rect, image._imageRep.AsCGImage(ref rect, null, null));
        return newImage;
    }

    protected override MacImage PerformTransform(MacImage image, ScaleTransform transform)
    {
        var width = (int) Math.Round(image.Width * transform.ScaleFactor);
        var height = (int) Math.Round(image.Height * transform.ScaleFactor);
        var pixelFormat = image.PixelFormat switch
        {
            ImagePixelFormat.BW1 or ImagePixelFormat.Gray8 => ImagePixelFormat.Gray8,
            ImagePixelFormat.RGB24 => ImagePixelFormat.ARGB32,
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
        if (image.PixelFormat is not (ImagePixelFormat.Gray8 or ImagePixelFormat.ARGB32))
        {
            // Only some formats supported for drawing, see "Pixel formats supported for bitmap graphics contexts"
            // https://developer.apple.com/library/archive/documentation/GraphicsImaging/Conceptual/drawingwithquartz2d/dq_context/dq_context.html#//apple_ref/doc/uid/TP30001066-CH203-BCIBHHBB
            throw new ArgumentException($"Unsupported pixel format for CGBitmapContext: {image.PixelFormat}");
        }
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
            ImagePixelFormat.RGB24 => ImagePixelFormat.ARGB32,
            ImagePixelFormat.ARGB32 => ImagePixelFormat.ARGB32,
            _ => throw new ArgumentException("Unsupported pixel format")
        };
        var (left, top, width, height) = transform.GetDrawRect(image.Width, image.Height);
        var newImage = (MacImage) ImageContext.Create(width, height, pixelFormat);
        newImage.SetResolution(
            image.HorizontalResolution * image.Width / width,
            image.VerticalResolution * image.Height / height);
        using CGBitmapContext c = GetCgBitmapContext(newImage);
        CGRect rect = new CGRect(0, 0, width, height);
        c.DrawImage(rect, image._imageRep.AsCGImage(ref rect, null, null));

        // CGRect strokeRect = new CGRect(left + 0.5, top + 0.5, width - 1, height - 1);
// #if MONOMAC
//         c.SetRGBStrokeColor(
// #else
//         c.SetStrokeColor(
// #endif
//             0.ToNFloat(), 0.ToNFloat(), 0.ToNFloat(), 255.ToNFloat());
//         c.StrokeRect(strokeRect);

        return newImage;
    }
}