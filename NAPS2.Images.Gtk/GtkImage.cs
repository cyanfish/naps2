using System.Globalization;
using Gdk;
using NAPS2.Images.Bitwise;
using NAPS2.Util;

namespace NAPS2.Images.Gtk;

public class GtkImage : IMemoryImage
{
    public GtkImage(ImageContext imageContext, Pixbuf pixbuf)
    {
        if (imageContext is not GtkImageContext) throw new ArgumentException("Expected GtkImageContext");
        LeakTracer.StartTracking(this);
        ImageContext = imageContext;
        Pixbuf = pixbuf;
        LogicalPixelFormat = PixelFormat;
        HorizontalResolution = float.TryParse(pixbuf.GetOption("x-dpi"), out var xDpi) ? xDpi : 0;
        VerticalResolution = float.TryParse(pixbuf.GetOption("y-dpi"), out var yDpi) ? yDpi : 0;
    }

    public ImageContext ImageContext { get; }

    public Pixbuf Pixbuf { get; }

    public int Width => Pixbuf.Width;

    public int Height => Pixbuf.Height;

    public float HorizontalResolution { get; private set; }

    public float VerticalResolution { get; private set; }

    public void SetResolution(float xDpi, float yDpi)
    {
        HorizontalResolution = xDpi;
        VerticalResolution = yDpi;
    }

    public ImagePixelFormat PixelFormat => (Pixbuf.NChannels, Pixbuf.BitsPerSample) switch
    {
        (3, 8) => ImagePixelFormat.RGB24,
        (4, 8) => ImagePixelFormat.ARGB32,
        _ => throw new InvalidOperationException("Unsupported pixel format")
    };

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        var ptr = Pixbuf.Pixels;
        var stride = Pixbuf.Rowstride;
        var subPixelType = PixelFormat switch
        {
            ImagePixelFormat.RGB24 => SubPixelType.Rgb,
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData(ptr, new PixelInfo(Width, Height, subPixelType, stride));
        return new GtkImageLockState();
    }

    // TODO: Should we implement some kind of actual locking?
    public class GtkImageLockState : ImageLockState
    {
        public override void Dispose()
        {
        }
    }

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified,
        ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        if (imageFormat == ImageFileFormat.Tiff)
        {
            ((GtkImageContext) ImageContext).TiffIo.SaveTiff([this], path);
            return;
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        options ??= new ImageSaveOptions();
        var type = GetType(imageFormat);
        var (keys, values) = GetSaveOptions(imageFormat, options.Quality);
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.RGB24);
        helper.Image.Pixbuf.Savev(path, type, keys, values);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        if (imageFormat == ImageFileFormat.Tiff)
        {
            ((GtkImageContext) ImageContext).TiffIo.SaveTiff([this], stream);
            return;
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        options ??= new ImageSaveOptions();
        var type = GetType(imageFormat);
        var (keys, values) = GetSaveOptions(imageFormat, options.Quality);
        // TODO: GDK doesn't support optimizing bit depth (e.g. 1bit/8bit instead of 24bit/32bit) for BMP/PNG/JPEG.
        // We'd probably need to use libpng/libjpeg etc. directly to fix that.
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.RGB24);
        // TODO: Map to OutputStream directly?
        stream.Write(helper.Image.Pixbuf.SaveToBuffer(type, keys, values));
    }

    private string GetType(ImageFileFormat fileFormat) => fileFormat switch
    {
        ImageFileFormat.Jpeg => "jpeg",
        ImageFileFormat.Png => "png",
        ImageFileFormat.Bmp => "bmp",
        ImageFileFormat.Tiff => "tiff",
        _ => throw new ArgumentException("Unsupported file format")
    };

    private (string[] keys, string[] values) GetSaveOptions(ImageFileFormat imageFormat, int quality)
    {
        var keys = new List<string>();
        var values = new List<string>();
        if (HorizontalResolution > 0 && VerticalResolution > 0)
        {
            keys.Add("x-dpi");
            keys.Add("y-dpi");
            values.Add(HorizontalResolution.ToString(CultureInfo.InvariantCulture));
            values.Add(VerticalResolution.ToString(CultureInfo.InvariantCulture));
        }
        if (imageFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            keys.Add("quality");
            values.Add(quality.ToString());
        }
        return (keys.ToArray(), values.ToArray());
    }

    public IMemoryImage Clone() => new GtkImage(ImageContext, (Pixbuf) Pixbuf.Clone())
    {
        OriginalFileFormat = OriginalFileFormat,
        LogicalPixelFormat = LogicalPixelFormat,
        HorizontalResolution = HorizontalResolution,
        VerticalResolution = VerticalResolution
    };

    public void Dispose()
    {
        Pixbuf.Dispose();
        LeakTracer.StopTracking(this);
    }
}