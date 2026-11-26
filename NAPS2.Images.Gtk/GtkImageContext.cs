using System.Threading;
using Gdk;
using NAPS2.Util;

namespace NAPS2.Images.Gtk;

public class GtkImageContext : ImageContext
{
    private readonly GtkImageTransformer _imageTransformer;
    private readonly LibTiffIo _tiffIo;

    public GtkImageContext() : base(typeof(GtkImage))
    {
        _imageTransformer = new GtkImageTransformer(this);
        _tiffIo = new LibTiffIo(this);
    }

    protected override bool SupportsDecodeCore(ImageFileFormat format) =>
        format is ImageFileFormat.Jpeg or ImageFileFormat.Png or ImageFileFormat.Bmp or ImageFileFormat.Gif
            or ImageFileFormat.Tiff
        || format == ImageFileFormat.Heic && (PixbufMimeTypes.Value.Contains("image/heic") ||
                                              PixbufMimeTypes.Value.Contains("image/heif"))
        || format == ImageFileFormat.Avif && PixbufMimeTypes.Value.Contains("image/avif");

    protected override bool SupportsEncodeCore(ImageFileFormat format) =>
        format is ImageFileFormat.Jpeg or ImageFileFormat.Png or ImageFileFormat.Bmp or ImageFileFormat.Tiff;

    // HEIC/AVIF are supported if the respective gdk-pixbuf loaders (e.g. from libheif or libavif) are installed
    private static readonly Lazy<HashSet<string>> PixbufMimeTypes = new(() =>
        Pixbuf.Formats.SelectMany(format => format.MimeTypes).ToHashSet());

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GtkImage ?? throw new ArgumentException("Expected GtkImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        if (format == ImageFileFormat.Tiff)
        {
            IMemoryImage image = null!;
            var cts = new CancellationTokenSource();
            _tiffIo.LoadTiff(img => { image = img; cts.Cancel(); }, stream, cts.Token);
            return image;
        }
        var pixbuf = new Pixbuf(stream);
        var gtkImage = new GtkImage(pixbuf);
        // Legacy gdk-pixbuf loaders provide the embedded color profile (base64-encoded) but don't apply it.
        // For modern glycin-based gdk-pixbuf, profiles are auto-applied and this option isn't set, so this code
        // path is ignored.
        if (pixbuf.GetOption("icc-profile") is { } iccProfile)
        {
            BundledColorConverter?.TryConvertToSrgb(gtkImage,Convert.FromBase64String(iccProfile));
        }
        return gtkImage;
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        if (format == ImageFileFormat.Tiff)
        {
            _tiffIo.LoadTiff(produceImage, stream, progress);
            return;
        }
        progress.Report(0, 1);
        if (progress.IsCancellationRequested) return;
        produceImage(LoadCore(stream, format));
        progress.Report(1, 1);
    }

    public override ITiffWriter TiffWriter => _tiffIo;

    internal LibTiffIo TiffIo => _tiffIo;

    public Pixbuf RenderToPixbuf(IRenderableImage image)
    {
        return ((GtkImage) Render(image)).Pixbuf;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unknown)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var pixbuf = new Pixbuf(Colorspace.Rgb, pixelFormat == ImagePixelFormat.ARGB32, 8, width, height);
        return new GtkImage(pixbuf);
    }
}