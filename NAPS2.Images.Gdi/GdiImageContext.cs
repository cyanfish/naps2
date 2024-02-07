using System.Drawing;
using System.Drawing.Imaging;
using NAPS2.Util;

namespace NAPS2.Images.Gdi;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
public class GdiImageContext : ImageContext
{
    private readonly GdiImageTransformer _imageTransformer;

    public GdiImageContext() : base(typeof(GdiImage))
    {
        _imageTransformer = new GdiImageTransformer(this);
    }

    protected override bool SupportsTiff => true;

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected GdiImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        var memoryStream = EnsureMemoryStream(stream);
        using var bitmap = new Bitmap(memoryStream);
        return new GdiImage(this, bitmap).Copy();
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        var memoryStream = EnsureMemoryStream(stream);
        using var bitmap = new Bitmap(memoryStream);
        int count = bitmap.GetFrameCount(FrameDimension.Page);
        for (int i = 0; i < count; i++)
        {
            progress.Report(i, count);
            if (progress.IsCancellationRequested) break;
            bitmap.SelectActiveFrame(FrameDimension.Page, i);
            produceImage(new GdiImage(this, bitmap).Copy());
        }
        progress.Report(count, count);
    }

    private static MemoryStream EnsureMemoryStream(Stream stream)
    {
        // Loading a bitmap directly from a file keeps a lock on the file, which we don't want.
        // Instead we can copy it to an in-memory stream first.
        if (stream is not MemoryStream memoryStream)
        {
            memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
        }
        return memoryStream;
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