using System.Threading;
using Gdk;

namespace NAPS2.Images.Gtk;

public class GtkImageContext : ImageContext
{
    private readonly GtkImageTransformer _imageTransformer;

    public GtkImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(GtkImage), pdfRenderer)
    {
        _imageTransformer = new GtkImageTransformer(this);
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
            return LoadTiff(stream, out count);
        }
        count = 1;
        return new[] { Load(stream) };
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var format = GetFileFormatFromExtension(path, true);
        if (format == ImageFileFormat.Tiff)
        {
            return LoadTiff(path, out count);
        }
        count = 1;
        return new[] { Load(path) };
    }

    public override bool SaveTiff(IList<IMemoryImage> images, string path, TiffCompressionType compression = TiffCompressionType.Auto,
        Action<int, int>? progressCallback = null, CancellationToken cancelToken = default)
    {
        throw new NotImplementedException();
    }

    public override bool SaveTiff(IList<IMemoryImage> images, Stream stream, TiffCompressionType compression = TiffCompressionType.Auto,
        Action<int, int>? progressCallback = null, CancellationToken cancelToken = default)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<IMemoryImage> LoadTiff(Stream stream, out int count)
    {
        var c = new LibTiffStreamClient(stream);
        var tiff = c.TIFFClientOpen("r");
        count = LibTiff.TIFFNumberOfDirectories(tiff);
        return EnumerateTiffFrames(tiff, c);
    }

    private IEnumerable<IMemoryImage> LoadTiff(string path, out int count)
    {
        var tiff = LibTiff.TIFFOpen(path, "r");
        count = LibTiff.TIFFNumberOfDirectories(tiff);
        return EnumerateTiffFrames(tiff, null);
    }

    private IEnumerable<IMemoryImage> EnumerateTiffFrames(IntPtr tiff, LibTiffStreamClient? client)
    {
        // We keep a reference to the client to avoid garbage collection
        try
        {
            do
            {
                LibTiff.TIFFGetField(tiff, 256, out var w);
                LibTiff.TIFFGetField(tiff, 257, out var h);
                var img = Create(w, h, ImagePixelFormat.ARGB32);
                img.OriginalFileFormat = ImageFileFormat.Tiff;
                using var imageLock = img.Lock(LockMode.WriteOnly, out var data);
                ReadTiffFrame(data.safePtr, tiff, w, h);
                yield return img;
            } while (LibTiff.TIFFReadDirectory(tiff) == 1);
        }
        finally
        {
            LibTiff.TIFFClose(tiff);
        }
    }

    private static void ReadTiffFrame(IntPtr buffer, IntPtr tiff, int w, int h)
    {
        int r = LibTiff.TIFFReadRGBAImageOriented(tiff, w, h, buffer, 1, 0);
        // TODO: Check return value
    }

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