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
        var macImage = image as MacImage ?? throw new ArgumentException("Expected MacImage object");
        return _imageTransformer.Apply(macImage, transform);
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
            var rep = MacBitmapHelper.CreateRep(width, height, pixelFormat);
            var image = new NSImage(rep.Size);
            image.AddRepresentation(rep);
            return new MacImage(this, image);
        }
    }
}