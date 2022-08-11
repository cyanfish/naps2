using MonoMac.AppKit;
using MonoMac.Foundation;

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
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(path);
        }
        if (image.Representations() == null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find image file '{path}'.");
            }
            throw new IOException($"Error reading image file '{path}'.");
        }
        var macImage = new MacImage(image);
        // TODO: Can we get the real format?
        macImage.OriginalFileFormat = GetFileFormatFromExtension(path, true);
        return macImage;
    }

    public override IMemoryImage Load(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
        lock (ConstructorLock)
        {
            return new MacImage(new NSImage(NSData.FromStream(stream)));
        }
    }

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        // TODO: Handle tiffs
        count = 1;
        return Enumerable.Repeat(Load(stream), 1);
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        // TODO: Handle tiffs
        count = 1;
        return Enumerable.Repeat(Load(path), 1);
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
                _ => throw new ArgumentException("Unsupported pixel format")
            };
            var image = new NSImage(rep.Size);
            image.AddRepresentation(rep);
            return new MacImage(image);
        }
    }
}