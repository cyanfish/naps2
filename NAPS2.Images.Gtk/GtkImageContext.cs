using Gdk;
using NAPS2.Util;

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

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        return new GtkImage(this, new Pixbuf(stream));
    }

    protected override void LoadFramesCore(AsyncSink<IMemoryImage> sink, Stream stream, ImageFileFormat format,
        ProgressHandler progress)
    {
        if (format == ImageFileFormat.Tiff)
        {
            _tiffIo.LoadTiff(sink, stream, progress);
            return;
        }
        progress.Report(0, 1);
        if (progress.IsCancellationRequested) return;
        sink.PutItem(LoadCore(stream, format));
        progress.Report(1, 1);
    }

    public override ITiffWriter TiffWriter => _tiffIo;

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var pixbuf = new Pixbuf(Colorspace.Rgb, pixelFormat == ImagePixelFormat.ARGB32, 8, width, height);
        return new GtkImage(this, pixbuf);
    }
}