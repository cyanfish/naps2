namespace NAPS2.Images.Mac;

internal static class MacBitmapHelper
{
    public static NSBitmapImageRep CopyRep(NSBitmapImageRep original, bool isGray, bool hasAlpha)
    {
        var w = original.PixelsWide;
        var h = original.PixelsHigh;
        var copy = CreateRepForDrawing(w, h, isGray, hasAlpha);
        using var c = CreateContext(copy, isGray, hasAlpha);
        CGRect rect = new CGRect(0, 0, w, h);
        c.DrawImage(rect, original.AsCGImage(ref rect, null, null));
        return copy;
    }

    public static NSBitmapImageRep CreateRep(long width, long height, ImagePixelFormat pixelFormat)
    {
        // TODO: Might want to change up reps - e.g. 32bit non-alpha for rgb.
        var samplesPerPixel = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => 4,
            ImagePixelFormat.RGB24 => 3,
            ImagePixelFormat.Gray8 => 1,
            ImagePixelFormat.BW1 => 1,
            _ => throw new ArgumentException("Unsupported pixel format")
        };
        var bitsPerSample = pixelFormat == ImagePixelFormat.BW1 ? 1 : 8;
        var colorSpace = pixelFormat is ImagePixelFormat.Gray8 or ImagePixelFormat.BW1
            ? NSColorSpace.DeviceWhite
            : NSColorSpace.DeviceRGB;
        bool hasAlpha = pixelFormat == ImagePixelFormat.ARGB32;
        return CreateRep(width, height, samplesPerPixel, bitsPerSample, colorSpace, hasAlpha);
    }

    public static NSBitmapImageRep CreateRepForDrawing(long width, long height, bool isGray, bool hasAlpha)
    {
        var samplesPerPixel = isGray ? 1 : 4;
        var colorSpace = isGray ? NSColorSpace.DeviceWhite : NSColorSpace.DeviceRGB;
        return CreateRep(width, height, samplesPerPixel, 8, colorSpace, hasAlpha);
    }

    private static NSBitmapImageRep CreateRep(long width, long height, int samplesPerPixel, int bitsPerSample,
        NSString colorSpace, bool hasAlpha)
    {
        lock (MacImageContext.ConstructorLock)
        {
            return new NSBitmapImageRep(
                IntPtr.Zero,
                width,
                height,
                bitsPerSample,
                samplesPerPixel,
                hasAlpha,
                false,
                colorSpace,
                samplesPerPixel * width,
                samplesPerPixel * bitsPerSample);
        }
    }

    public static CGBitmapContext CreateContext(MacImage image)
    {
        // TODO: We probably want to support PixelFormat=RGB24 here provided the actual rep is 32 bits (with alpha=none)
        if (image.PixelFormat is not (ImagePixelFormat.Gray8 or ImagePixelFormat.ARGB32))
        {
            // Only some formats supported for drawing, see "Pixel formats supported for bitmap graphics contexts"
            // https://developer.apple.com/library/archive/documentation/GraphicsImaging/Conceptual/drawingwithquartz2d/dq_context/dq_context.html#//apple_ref/doc/uid/TP30001066-CH203-BCIBHHBB
            throw new ArgumentException($"Unsupported pixel format for CGBitmapContext: {image.PixelFormat}");
        }
        bool isGray = image.PixelFormat == ImagePixelFormat.Gray8;
        return CreateContext(image._imageRep, isGray, !isGray);
    }

    public static CGBitmapContext CreateContext(NSBitmapImageRep rep, bool isGray, bool hasAlpha)
    {
        var colorSpace = isGray
            ? CGColorSpace.CreateDeviceGray()
            : CGColorSpace.CreateDeviceRGB();
        var alphaInfo = hasAlpha
            ? CGImageAlphaInfo.PremultipliedLast
            : CGImageAlphaInfo.None;
        lock (MacImageContext.ConstructorLock)
        {
            return new CGBitmapContext(
                rep.BitmapData,
                (int) rep.PixelsWide,
                (int) rep.PixelsHigh,
                (int) rep.BitsPerSample,
                (int) rep.BytesPerRow,
                colorSpace,
                alphaInfo);
        }
    }
}