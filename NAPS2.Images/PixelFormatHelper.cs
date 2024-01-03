namespace NAPS2.Images;

// TODO: Use this for TIFF saving too, maybe
internal static class PixelFormatHelper
{
    public static PixelFormatHelper<T> Create<T>(T image,
        ImagePixelFormat targetFormat = ImagePixelFormat.Unsupported,
        ImagePixelFormat minFormat = ImagePixelFormat.Unsupported) where T : IMemoryImage
    {
        return new PixelFormatHelper<T>(image, targetFormat, minFormat);
    }
}

internal class PixelFormatHelper<T> : IDisposable where T : IMemoryImage
{
    public PixelFormatHelper(T image, ImagePixelFormat targetFormat, ImagePixelFormat minFormat)
    {
        image.UpdateLogicalPixelFormat();
        // TODO: Maybe we can be aware of the target filetype, e.g. JPEG doesn't have 1bpp. Although the specifics
        // are going to be platform-dependent.
        if (targetFormat == ImagePixelFormat.Unsupported)
        {
            // If targetFormat is not specified, we'll use the logical format to minimize on-disk size.
            targetFormat = image.LogicalPixelFormat;
        }
        if (targetFormat < image.LogicalPixelFormat)
        {
            // We never want to lose color information.
            targetFormat = image.LogicalPixelFormat;
        }
        if (targetFormat < minFormat)
        {
            // GTK only supports RGB24/ARGB32 so it's pointless to target BW1/Gray8 as it will end up as RGB24 anyway.
            targetFormat = minFormat;
        }

        if (targetFormat != image.PixelFormat)
        {
            Image = (T) image.CopyWithPixelFormat(targetFormat);
            IsCopy = true;
        }
        else
        {
            Image = image;
            IsCopy = false;
        }
    }

    public T Image { get; }

    public bool IsCopy { get; }

    public void Dispose()
    {
        if (IsCopy)
        {
            Image.Dispose();
        }
    }
}