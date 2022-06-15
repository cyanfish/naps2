using System.Drawing;
using System.Drawing.Imaging;

namespace NAPS2.Images.Gdi;

public class GdiImageContext : ImageContext
{
    public GdiImageContext() : base(typeof(GdiImage))
    {
        RegisterTransformers<GdiImage>(new GdiTransformers());
        // TODO: Not sure where to do these
        // RegisterConverters(new PdfConverters(this));
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

    public override IMemoryImage Render(ProcessedImage processedImage)
    {
        return new GdiImage(RenderToBitmap(processedImage));
    }

    public Bitmap RenderToBitmap(ProcessedImage processedImage)
    {
        // TODO: Need to take transforms into account
        switch (processedImage.Storage)
        {
            // TODO: We probably want to support PDFs somehow (which presumably use fileStorage?)
            case ImageFileStorage fileStorage:
                // Rather than creating a bitmap from the file directly, instead we read it into memory first.
                // This ensures we don't accidentally keep a lock on the storage file, which would cause an error if we
                // try to delete it before the bitmap is disposed.
                // This is less efficient in the case where the bitmap is guaranteed to be disposed quickly, but for now
                // that seems like a reasonable tradeoff to avoid a whole class of hard-to-diagnose errors.
                var stream = new MemoryStream(File.ReadAllBytes(fileStorage.FullPath));
                return new Bitmap(stream);
            case MemoryStreamImageStorage memoryStreamStorage:
                return new Bitmap(memoryStreamStorage.Stream);
            case GdiImage image:
                return image.Clone().AsBitmap();
        }
        throw new ArgumentException("Unsupported image storage: " + processedImage.Storage);
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

    public override string SaveSmallestFormat(IMemoryImage image, string pathWithoutExtension, BitDepth bitDepth, bool highQuality, int quality, out ImageFileFormat imageFileFormat)
    {
        var result = ScannedImageHelper.SaveSmallestBitmap(image.AsBitmap(), pathWithoutExtension, bitDepth, highQuality, quality, out var imageFormat);
        imageFileFormat = imageFormat.AsImageFileFormat();
        return result;
    }
}
