using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
public class GdiImageContext : ImageContext
{
    private readonly GdiImageTransformer _imageTransformer;

    public GdiImageContext() : this(null)
    {
    }

    public GdiImageContext(IPdfRenderer? pdfRenderer) : base(typeof(GdiImage), pdfRenderer)
    {
        _imageTransformer = new GdiImageTransformer(this);
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected GdiImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        stream = EnsureMemoryStream(stream);
        return new GdiImage(this, new Bitmap(stream));
    }

    protected override IEnumerable<IMemoryImage> LoadFramesCore(Stream stream, ImageFileFormat format, out int count)
    {
        stream = EnsureMemoryStream(stream);
        var bitmap = new Bitmap(stream);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    private static Stream EnsureMemoryStream(Stream stream)
    {
        // Loading a bitmap directly from a file keeps a lock on the file, which we don't want.
        // Instead we can copy it to an in-memory stream first.
        if (stream is not MemoryStream)
        {
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            stream = memoryStream;
        }
        return stream;
    }

    private IEnumerable<IMemoryImage> EnumerateFrames(Bitmap bitmap, int count)
    {
        using (bitmap)
        {
            for (int i = 0; i < count; i++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Page, i);
                yield return new GdiImage(this, bitmap).Copy();
            }
        }
    }

    public override ITiffWriter TiffWriter { get; } = new GdiTiffWriter();

    public Bitmap RenderToBitmap(IRenderableImage image)
    {
        return ((GdiImage) Render(image)).Bitmap;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        var bitmap = new Bitmap(width, height, pixelFormat.AsPixelFormat());
        if (pixelFormat == ImagePixelFormat.BW1)
        {
            var p = bitmap.Palette;
            p.Entries[0] = Color.Black;
            p.Entries[1] = Color.White;
            bitmap.Palette = p;
        }
        if (pixelFormat == ImagePixelFormat.Gray8)
        {
            var p = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                p.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = p;
        }
        return new GdiImage(this, bitmap);
    }
}