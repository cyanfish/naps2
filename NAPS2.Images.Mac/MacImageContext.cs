namespace NAPS2.Images.Mac;

public class MacImageContext : ImageContext
{
    // We need to lock around MonoMac constructors as they aren't thread safe
    internal static readonly object ConstructorLock = new object();
    
    private readonly MacImageTransformer _imageTransformer;
    
    public MacImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(MacImage), pdfRenderer)
    {
        // TODO: Not sure if this is truly thread safe.
        // Maybe we need to do something like use CGImage with a custom context for thread safety.
        NSApplication.CheckForIllegalCrossThreadCalls = false;
        _imageTransformer = new MacImageTransformer(this);
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as MacImage ?? throw new ArgumentException("Expected MacImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path)
    {
        using var readLock = new FileStream(path, FileMode.Open, FileAccess.Read);
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(path);
        }
        CheckReps(path, image);
        return new MacImage(this, image)
        {
            OriginalFileFormat = GetFileFormatFromExtension(path, true)
        };
    }

    public override IMemoryImage Load(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
        lock (ConstructorLock)
        {
            var image = new NSImage(NSData.FromStream(stream) ?? throw new ArgumentException(nameof(stream)));
            return new MacImage(this, image)
            {
                OriginalFileFormat = GetFileFormatFromFirstBytes(stream)
            };
        }
    }

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(NSData.FromStream(stream) ?? throw new ArgumentException(nameof(stream)));
        }
        count = image.Representations().Length;
        return SplitFrames(image, GetFileFormatFromFirstBytes(stream));
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        using var readLock = new FileStream(path, FileMode.Open, FileAccess.Read);
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(path);
        }
        CheckReps(path, image);
        count = image.Representations().Length;
        return SplitFrames(image, GetFileFormatFromExtension(path, true));
    }

    public override ITiffWriter TiffWriter => throw new NotImplementedException();

    private IEnumerable<IMemoryImage> SplitFrames(NSImage image, ImageFileFormat fileFormat)
    {
        foreach (var rep in image.Representations())
        {
            NSImage frame;
            lock (ConstructorLock)
            {
                frame = new NSImage(rep.Size);
            }
            frame.AddRepresentation(rep);
            yield return new MacImage(this, frame)
            {
                OriginalFileFormat = fileFormat
            };
        }
    }

    private static void CheckReps(string path, NSImage image)
    {
        if (image.Representations() == null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find image file '{path}'.");
            }
            throw new IOException($"Error reading image file '{path}'.");
        }
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        // TODO: Can we support 1bpp?
        lock (ConstructorLock)
        {
            var rep = pixelFormat switch
            {
                ImagePixelFormat.ARGB32 => new NSBitmapImageRep(
                    IntPtr.Zero, width, height, 8, 4, true, false, NSColorSpace.DeviceRGB, 4 * width, 32),
                ImagePixelFormat.RGB24 => new NSBitmapImageRep(
                    IntPtr.Zero, width, height, 8, 3, false, false, NSColorSpace.DeviceRGB, 3 * width, 24),
                ImagePixelFormat.Gray8 => new NSBitmapImageRep(
                    IntPtr.Zero, width, height, 8, 1, false, false, NSColorSpace.DeviceWhite, width, 8),
                ImagePixelFormat.BW1 => new NSBitmapImageRep(
                    IntPtr.Zero, width, height, 1, 1, false, false, NSColorSpace.DeviceWhite, (width + 7) / 8, 1),
                _ => throw new ArgumentException("Unsupported pixel format")
            };
            var image = new NSImage(rep.Size);
            image.AddRepresentation(rep);
            return new MacImage(this, image);
        }
    }
}