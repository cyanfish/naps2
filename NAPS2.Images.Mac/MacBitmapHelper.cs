namespace NAPS2.Images.Mac;

internal static class MacBitmapHelper
{
    public static NSBitmapImageRep CopyRep(NSBitmapImageRep original)
    {
        var w = original.PixelsWide;
        var h = original.PixelsHigh;
        // TODO: Consider creating something other than ARGB32 images based on the original rep
        // Though it doesn't matter that much as we're not hitting this case in practice.
        var copy = CreateRepForDrawing(w, h);
        using var c = CreateContext(copy, false, true);
        CGRect rect = new CGRect(0, 0, w, h);
        c.DrawImage(rect, original.AsCGImage(ref rect, null, null));
        return copy;
    }

    public static NSBitmapImageRep CreateRep(long width, long height, ImagePixelFormat pixelFormat)
    {
        var samplesPerPixel = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => 4,
            ImagePixelFormat.RGB24 => 4,
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

    public static NSBitmapImageRep CreateRepForDrawing(long width, long height)
    {
        return CreateRep(width, height, 4, 8, NSColorSpace.DeviceRGB, true);
    }

    private static NSBitmapImageRep CreateRep(long width, long height, int samplesPerPixel, int bitsPerSample,
        NSString colorSpace, bool hasAlpha)
    {
        lock (MacImageContext.ConstructorLock)
        {
            var realSamples = samplesPerPixel == 4 && !hasAlpha ? 3 : samplesPerPixel;
            return new NSBitmapImageRep(
                IntPtr.Zero,
                width,
                height,
                bitsPerSample,
                realSamples,
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
        var bytesPerPixel = rep.BytesPerRow / rep.PixelsWide;
        var alphaInfo = hasAlpha
            ? CGImageAlphaInfo.PremultipliedLast
            : bytesPerPixel is 4 or 8
                ? CGImageAlphaInfo.NoneSkipLast
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