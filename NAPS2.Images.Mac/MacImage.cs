using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Mac;

public class MacImage : IMemoryImage
{
    public MacImage(ImageContext imageContext, NSImage image)
    {
        if (imageContext is not MacImageContext) throw new ArgumentException("Expected MacImageContext");
        ImageContext = imageContext;
        NsImage = image ?? throw new ArgumentNullException(nameof(image));
        var reps = NsImage.Representations();
        if (reps.Length != 1)
        {
            foreach (var rep in reps)
            {
                rep.Dispose();
            }
            throw new ArgumentException("Expected NSImage with exactly one representation");
        }
        lock (MacImageContext.ConstructorLock)
        {
#if MONOMAC
            Rep = new NSBitmapImageRep(reps[0].Handle, false);
#else
            Rep = (NSBitmapImageRep) reps[0];
#endif
        }
        PixelFormat = GetPixelFormat(Rep);
        // TODO: Why do we get an unsupported warning for ColorSpaceName?
        bool isDeviceColorSpace = Rep.ColorSpaceName == NSColorSpace.DeviceRGB ||
                                  Rep.ColorSpaceName == NSColorSpace.DeviceWhite;
        if (PixelFormat == ImagePixelFormat.Unsupported)
        {
            var rep = MacBitmapHelper.CopyRep(Rep);
            ReplaceRep(rep);
        }
        else if (!isDeviceColorSpace)
        {
            var newColorSpace = Rep.ColorSpace.ColorComponents == 1
                ? NSColorSpace.DeviceGrayColorSpace
                : NSColorSpace.DeviceRGBColorSpace;
            var rep = Rep.ConvertingToColorSpace(newColorSpace, NSColorRenderingIntent.Default);
            ReplaceRep(rep);
        }
        LogicalPixelFormat = PixelFormat;
    }

    private void ReplaceRep(NSBitmapImageRep rep)
    {
        NsImage.RemoveRepresentation(Rep);
        Rep.Dispose();
        Rep = rep;
        NsImage.AddRepresentation(Rep);
        PixelFormat = GetPixelFormat(Rep);
    }

    private static ImagePixelFormat GetPixelFormat(NSBitmapImageRep rep)
    {
        return rep switch
        {
            { BitsPerPixel: 32, BitsPerSample: 8, SamplesPerPixel: 4 } => ImagePixelFormat.ARGB32,
            { BitsPerPixel: 32, BitsPerSample: 8, SamplesPerPixel: 3 } => ImagePixelFormat.RGB24,
            { BitsPerPixel: 8, BitsPerSample: 8, SamplesPerPixel: 1 } => ImagePixelFormat.Gray8,
            { BitsPerPixel: 1, BitsPerSample: 1, SamplesPerPixel: 1 } => ImagePixelFormat.BW1,
            _ => ImagePixelFormat.Unsupported
        };
    }

    public ImageContext ImageContext { get; }

    public NSImage NsImage { get; }

    internal NSBitmapImageRep Rep { get; private set; }

    public int Width => (int) Rep.PixelsWide;
    public int Height => (int) Rep.PixelsHigh;
    public float HorizontalResolution => (float) NsImage.Size.Width.ToDouble() / Width * 72;
    public float VerticalResolution => (float) NsImage.Size.Height.ToDouble() / Height * 72;

    public void SetResolution(float xDpi, float yDpi)
    {
        // TODO: Image size or imagerep size?
        if (xDpi > 0 && yDpi > 0)
        {
            NsImage.Size = new CGSize(xDpi / 72 * Width, yDpi / 72 * Height);
        }
    }

    public ImagePixelFormat PixelFormat { get; private set; }

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        var ptr = Rep.BitmapData;
        var stride = (int) Rep.BytesPerRow;
        var subPixelType = PixelFormat switch
        {
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            ImagePixelFormat.RGB24 => SubPixelType.Rgbn,
            ImagePixelFormat.Gray8 => SubPixelType.Gray,
            ImagePixelFormat.BW1 => SubPixelType.Bit,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData(ptr, new PixelInfo(Width, Height, subPixelType, stride));
        return new MacImageLockState();
    }

    // TODO: Should we implement some kind of actual locking?
    public class MacImageLockState : ImageLockState
    {
        public override void Dispose()
        {
        }
    }

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        var rep = GetRepForSaving(imageFormat, quality);
        if (!rep.Save(path, false, out var error))
        {
            throw new IOException(error!.Description);
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        var rep = GetRepForSaving(imageFormat, quality);
        rep.AsStream().CopyTo(stream);
    }

    private NSData GetRepForSaving(ImageFileFormat imageFormat, int quality)
    {
        lock (MacImageContext.ConstructorLock)
        {
            var props = quality != -1 && imageFormat is ImageFileFormat.Jpeg or ImageFileFormat.Jpeg2000
                ? NSDictionary.FromObjectAndKey(NSNumber.FromDouble(quality / 100.0),
                    NSBitmapImageRep.CompressionFactor)
                : null;
            var fileType = imageFormat switch
            {
                ImageFileFormat.Jpeg => NSBitmapImageFileType.Jpeg,
                ImageFileFormat.Png => NSBitmapImageFileType.Png,
                ImageFileFormat.Bmp => NSBitmapImageFileType.Bmp,
                ImageFileFormat.Tiff => NSBitmapImageFileType.Tiff,
                ImageFileFormat.Jpeg2000 => NSBitmapImageFileType.Jpeg2000,
                _ => throw new InvalidOperationException("Unsupported image format")
            };
            var targetFormat = LogicalPixelFormat;
            if (imageFormat == ImageFileFormat.Bmp && targetFormat == ImagePixelFormat.Gray8)
            {
                // Workaround for NSImage issue saving 8bit BMPs
                targetFormat = ImagePixelFormat.RGB24;
            }
            if (targetFormat != PixelFormat)
            {
                // We only want to save with the needed color info to minimize file sizes
                using var copy = (MacImage) this.CopyWithPixelFormat(targetFormat);
                return copy.Rep.RepresentationUsingTypeProperties(fileType, props);
            }
            return Rep.RepresentationUsingTypeProperties(fileType, props);
        }
    }

    public IMemoryImage Clone()
    {
        lock (MacImageContext.ConstructorLock)
        {
            if (PixelFormat == ImagePixelFormat.BW1)
            {
                // Workaround for NSImage issue copying 1bit images
                return this.Copy();
            }

#if MONOMAC
            var nsImage = new NSImage(NsImage.Copy().Handle, true);
#else
            var nsImage = (NSImage) NsImage.Copy();
#endif
            return new MacImage(ImageContext, nsImage)
            {
                OriginalFileFormat = OriginalFileFormat,
                LogicalPixelFormat = LogicalPixelFormat
            };
        }
    }

    public void Dispose()
    {
        Rep.Dispose();
        NsImage.Dispose();
    }
}