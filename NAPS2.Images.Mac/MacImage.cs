using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Mac;

public class MacImage : IMemoryImage
{
    public MacImage(NSImage image)
    {
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
        // TODO: Any replacement for deprecated ColorSpaceName?
#pragma warning disable CA1416,CA1422
        bool isDeviceColorSpace = Rep.ColorSpaceName == NSColorSpace.DeviceRGB ||
                                  Rep.ColorSpaceName == NSColorSpace.DeviceWhite;
#pragma warning restore CA1416,CA1422
        if (PixelFormat == ImagePixelFormat.Unknown)
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
            rep.Size = Rep.Size;
            ReplaceRep(rep);
        }
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
            _ => ImagePixelFormat.Unknown
        };
    }

    public ImageContext ImageContext { get; } = new MacImageContext();

    public NSImage NsImage { get; }

    internal NSBitmapImageRep Rep { get; private set; }

    public int Width => (int) Rep.PixelsWide;
    public int Height => (int) Rep.PixelsHigh;
    public float HorizontalResolution => (float) (Width / NsImage.Size.Width * 72).ToDouble();
    public float VerticalResolution => (float) (Height / NsImage.Size.Height * 72).ToDouble();

    public void SetResolution(float xDpi, float yDpi)
    {
        if (xDpi > 0 && yDpi > 0)
        {
            NsImage.Size = Rep.Size = new CGSize(Width / xDpi * 72, Height / yDpi * 72);
        }
    }

    public ImagePixelFormat PixelFormat { get; private set; }

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        if (lockMode != LockMode.ReadOnly)
        {
            LogicalPixelFormat = ImagePixelFormat.Unknown;
        }
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

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unknown,
        ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unknown)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        var rep = GetRepForSaving(imageFormat, options);
        if (!rep.Save(path, false, out var error))
        {
            throw new IOException(error!.Description);
        }
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unknown)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        var rep = GetRepForSaving(imageFormat, options);
        rep.AsStream().CopyTo(stream);
    }

    private NSData GetRepForSaving(ImageFileFormat imageFormat, ImageSaveOptions? options)
    {
        options ??= new ImageSaveOptions();
        lock (MacImageContext.ConstructorLock)
        {
            var fileType = imageFormat switch
            {
                // TODO: Any replacement for deprecated UTType?
#pragma warning disable CA1416,CA1422
                ImageFileFormat.Jpeg => UTType.JPEG,
                ImageFileFormat.Png => UTType.PNG,
                ImageFileFormat.Bmp => UTType.BMP,
                ImageFileFormat.Tiff => UTType.TIFF,
                ImageFileFormat.Jpeg2000 => UTType.JPEG2000,
#pragma warning restore CA1416,CA1422
                _ => throw new InvalidOperationException("Unsupported image format")
            };
            var targetFormat = options.PixelFormatHint;
            if (imageFormat == ImageFileFormat.Bmp && targetFormat == ImagePixelFormat.Unknown &&
                PixelFormat == ImagePixelFormat.Gray8)
            {
                // Workaround for issue in some macOS versions with 8bit BMPs
                targetFormat = ImagePixelFormat.RGB24;
            }
            using var helper = PixelFormatHelper.Create(this, targetFormat);
            var cgImage = helper.Image.Rep.CGImage; //RepresentationUsingTypeProperties(fileType, props);
            var data = new NSMutableData();
            var props = new NSMutableDictionary();
            props.Add((NSString) "DPIWidth", NSObject.FromObject(HorizontalResolution));
            props.Add((NSString) "DPIHeight", NSObject.FromObject(VerticalResolution));
            if (options.Quality != -1 && imageFormat is ImageFileFormat.Jpeg or ImageFileFormat.Jpeg2000)
            {
                props.Add((NSString) "kCGImageDestinationLossyCompressionQuality", NSNumber.FromFloat(options.Quality / 100.0f));
            }
#if MONOMAC
            using var dest = CGImageDestination.FromData(data, fileType, 1);
#else
            using var dest = CGImageDestination.Create(data, fileType.ToString(), 1)!;
#endif
            dest.AddImage(cgImage, props);
            dest.Close();
            return data;
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
            return new MacImage(nsImage)
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