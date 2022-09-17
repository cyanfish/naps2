using System.Globalization;
using Gdk;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gtk;

public class GtkImage : IMemoryImage
{
    public GtkImage(ImageContext imageContext, Pixbuf pixbuf, ImagePixelFormat logicalPixelFormat)
    {
        ImageContext = imageContext ?? throw new ArgumentNullException(nameof(imageContext));
        Pixbuf = pixbuf;
        LogicalPixelFormat = logicalPixelFormat;
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

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        var type = GetType(imageFormat);
        var (keys, values) = GetSaveOptions(imageFormat, quality);
        Pixbuf.Savev(path, type, keys, values);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, int quality = -1)
    {
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        var type = GetType(imageFormat);
        var (keys, values) = GetSaveOptions(imageFormat, quality);
        // TODO: Map to OutputStream directly?
        stream.Write(Pixbuf.SaveToBuffer(type, keys, values));
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
        var keys = new List<string>
        {
            "x-dpi",
            "y-dpi"
        };
        var values = new List<string>
        {
            HorizontalResolution.ToString(CultureInfo.InvariantCulture),
            VerticalResolution.ToString(CultureInfo.InvariantCulture)
        };
        if (imageFormat == ImageFileFormat.Jpeg && quality != -1)
        {
            keys.Add("quality");
            values.Add(quality.ToString());
        }
        return (keys.ToArray(), values.ToArray());
    }

    public IMemoryImage Clone() => new GtkImage(ImageContext, (Pixbuf) Pixbuf.Clone(), LogicalPixelFormat)
    {
        OriginalFileFormat = OriginalFileFormat
    };

    public IMemoryImage SafeClone() => Clone();

    public void Dispose() => Pixbuf.Dispose();
}