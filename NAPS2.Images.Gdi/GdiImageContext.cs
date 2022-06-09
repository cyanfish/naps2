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

    public override IMemoryImage Load(string path) => new GdiImage(new Bitmap(path));

    public override IMemoryImage Load(Stream stream) => new GdiImage(new Bitmap(stream));

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        var bitmap = new Bitmap(stream);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var bitmap = new Bitmap(path);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, count);
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

    public override IMemoryImage Render(ProcessedImage processedImage) => processedImage.RenderToImage();

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
