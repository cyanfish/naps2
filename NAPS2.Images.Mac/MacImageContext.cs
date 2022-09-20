namespace NAPS2.Images.Mac;

public class MacImageContext : ImageContext
{
    // We need to lock around MonoMac constructors as they aren't thread safe
    internal static readonly object ConstructorLock = new object();
    
    private readonly MacImageTransformer _imageTransformer;
    
    public MacImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(MacImage), pdfRenderer)
    {
        // TODO: Not sure if this is truly thread safe.
        NSApplication.CheckForIllegalCrossThreadCalls = false;
        _imageTransformer = new MacImageTransformer(this);
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as MacImage ?? throw new ArgumentException("Expected MacImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        lock (ConstructorLock)
        {
            var image = new NSImage(NSData.FromStream(stream) ?? throw new ArgumentException(nameof(stream)));
            return new MacImage(this, image);
        }
    }

    protected override IEnumerable<IMemoryImage> LoadFramesCore(Stream stream, ImageFileFormat format, out int count)
    {
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(NSData.FromStream(stream) ?? throw new ArgumentException(nameof(stream)));
        }
        count = image.Representations().Length;
        return SplitFrames(image);
    }

    public override ITiffWriter TiffWriter => throw new NotImplementedException();

    private IEnumerable<IMemoryImage> SplitFrames(NSImage image)
    {
        foreach (var rep in image.Representations())
        {
            NSImage frame;
            lock (ConstructorLock)
            {
                frame = new NSImage(rep.Size);
            }
            frame.AddRepresentation(rep);
            yield return new MacImage(this, frame);
        }
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
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