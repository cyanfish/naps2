using NAPS2.Util;

namespace NAPS2.Images.Mac;

public class MacImageContext : ImageContext
{
    // We need to lock around MonoMac constructors as they aren't thread safe
    internal static readonly object ConstructorLock = new object();

    private readonly MacImageTransformer _imageTransformer;

    public MacImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(MacImage), pdfRenderer)
    {
        NSApplication.CheckForIllegalCrossThreadCalls = false;
        _imageTransformer = new MacImageTransformer(this);
    }

    protected override bool SupportsTiff => true;
    protected override bool SupportsJpeg2000 => true;

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
            var reps = image.Representations();
            try
            {
                if (reps.Length > 1)
                {
                    return CreateImage(reps[0]);
                }
                return new MacImage(this, image);
            }
            finally
            {
                foreach (var rep in reps)
                {
                    rep.Dispose();
                }
            }
        }
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        NSImage image;
        lock (ConstructorLock)
        {
            image = new NSImage(NSData.FromStream(stream) ?? throw new ArgumentException(nameof(stream)));
        }
        var reps = image.Representations();
        try
        {
            for (int i = 0; i < reps.Length; i++)
            {
                progress.Report(i, reps.Length);
                if (progress.IsCancellationRequested) break;
                produceImage(CreateImage(reps[i]));
            }
            progress.Report(reps.Length, reps.Length);
        }
        finally
        {
            foreach (var rep in reps)
            {
                rep.Dispose();
            }
        }
    }

    public override ITiffWriter TiffWriter { get; } = new MacTiffWriter();

    public NSImage RenderToNsImage(IRenderableImage image)
    {
        return ((MacImage) Render(image)).NsImage;
    }

    private IMemoryImage CreateImage(NSImageRep rep)
    {
        NSImage frame;
        lock (ConstructorLock)
        {
            frame = new NSImage(rep.Size);
        }
        frame.AddRepresentation(rep);
        return new MacImage(this, frame);
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        lock (ConstructorLock)
        {
            var rep = MacBitmapHelper.CreateRep(width, height, pixelFormat);
            var image = new NSImage(rep.Size);
            image.AddRepresentation(rep);
            rep.Dispose();
            return new MacImage(this, image);
        }
    }
}