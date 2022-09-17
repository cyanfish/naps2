using Gdk;

namespace NAPS2.Images.Gtk;

public class GtkImageContext : ImageContext
{
    private readonly GtkImageTransformer _imageTransformer;
    private readonly LibTiffIo _tiffIo;

    public GtkImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(GtkImage), pdfRenderer)
    {
        _imageTransformer = new GtkImageTransformer(this);
        _tiffIo = new LibTiffIo(this);
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GtkImage ?? throw new ArgumentException("Expected GtkImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path)
    {
        using var readStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var pixbuf = new Pixbuf(readStream);
        return new GtkImage(this, pixbuf, pixbuf.HasAlpha ? ImagePixelFormat.ARGB32 : ImagePixelFormat.RGB24)
        {
            OriginalFileFormat = GetFileFormatFromExtension(path, true)
        };
    }

    public override IMemoryImage Load(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
        var pixbuf = new Pixbuf(stream);
        return new GtkImage(this, pixbuf, pixbuf.HasAlpha ? ImagePixelFormat.ARGB32 : ImagePixelFormat.RGB24)
        {
            OriginalFileFormat = GetFileFormatFromFirstBytes(stream)
        };
    }

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        if (format == ImageFileFormat.Tiff)
        {
            return _tiffIo.LoadTiff(stream, out count);
        }
        count = 1;
        return new[] { Load(stream) };
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var format = GetFileFormatFromExtension(path, true);
        if (format == ImageFileFormat.Tiff)
        {
            return _tiffIo.LoadTiff(path, out count);
        }
        count = 1;
        return new[] { Load(path) };
    }

    public override ITiffWriter TiffWriter => _tiffIo;

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var pixbuf = new Pixbuf(Colorspace.Rgb, pixelFormat == ImagePixelFormat.ARGB32, 8, width, height);
        return new GtkImage(this, pixbuf, pixelFormat);
    }
}