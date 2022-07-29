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
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected GdiImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path) => new GdiImage(LoadBitmapWithExceptionHandling(path));

    public override IMemoryImage Load(Stream stream) => new GdiImage(new Bitmap(stream));

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
                yield return new GdiImage((Bitmap) bitmap.Clone());
            }
        }
    }

    public Bitmap RenderToBitmap(ProcessedImage processedImage)
    {
        return ((GdiImage) Render(processedImage)).Bitmap;
    }

    public override IMemoryImage RenderFromStorage(IImageStorage storage)
    {
        switch (storage)
        {
            case ImageFileStorage fileStorage:
                if (MaybeRenderPdf(fileStorage, out var renderedPdf))
                {
                    return renderedPdf!;
                }
                // Rather than creating a bitmap from the file directly, instead we read it into memory first.
                // This ensures we don't accidentally keep a lock on the storage file, which would cause an error if we
                // try to delete it before the bitmap is disposed.
                // This is less efficient in the case where the bitmap is guaranteed to be disposed quickly, but for now
                // that seems like a reasonable tradeoff to avoid a whole class of hard-to-diagnose errors.
                var stream = new MemoryStream(File.ReadAllBytes(fileStorage.FullPath));
                return new GdiImage(new Bitmap(stream));
            case ImageMemoryStorage memoryStorage:
                if (MaybeRenderPdf(memoryStorage, out var renderedMemoryPdf))
                {
                    return renderedMemoryPdf!;
                }
                return new GdiImage(new Bitmap(memoryStorage.Stream));
            case GdiImage image:
                return image.Clone();
        }
        throw new ArgumentException("Unsupported image storage: " + storage);
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
        return new GdiImage(bitmap);
    }
}
