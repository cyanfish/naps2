using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public class GdiImageContext : ImageContext
{
    private readonly GdiImageTransformer _imageTransformer;
    
    public GdiImageContext() : this(null)
    {
    }

    public GdiImageContext(IPdfRenderer? pdfRenderer) : base(typeof(GdiImage), pdfRenderer)
    {
        _imageTransformer = new GdiImageTransformer(this);
        LoadFromFileKeepsLock = true;
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected GdiImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path) => new GdiImage(this, LoadBitmapWithExceptionHandling(path));

    public override IMemoryImage Load(Stream stream) => new GdiImage(this, new Bitmap(stream));

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        var bitmap = new Bitmap(stream);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var bitmap = LoadBitmapWithExceptionHandling(path);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    private static Bitmap LoadBitmapWithExceptionHandling(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch (ArgumentException)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find image file '{path}'.");
            }
            throw new IOException($"Error reading image file '{path}'.");
        }
    }

    private IEnumerable<IMemoryImage> EnumerateFrames(Bitmap bitmap, int count)
    {
        using (bitmap)
        {
            for (int i = 0; i < count; i++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Page, i);
                yield return new GdiImage(this, (Bitmap) bitmap.Clone());
            }
        }
    }

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
