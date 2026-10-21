using System.Windows.Media.Imaging;
using NAPS2.Util;

namespace NAPS2.Images.Wpf;

public class WpfImage : IMemoryImage
{
    private readonly WpfPixelBuffer _buffer;
    private bool _disposed;
    private double _xDpi, _yDpi;

    public WpfImage(BitmapSource bitmap) : this(WpfPixelBuffer.FromSource(bitmap), bitmap.DpiX, bitmap.DpiY)
    {
    }

    internal WpfImage(WpfPixelBuffer buffer, double xDpi, double yDpi)
    {
        LeakTracer.StartTracking(this);
        _buffer = buffer;
        _xDpi = xDpi;
        _yDpi = yDpi;
    }

    public ImageContext ImageContext { get; } = new WpfImageContext();

    public BitmapSource ToBitmapSource()
    {
        if (_disposed) throw new InvalidOperationException();
        return _buffer.ToBitmapSource(_xDpi, _yDpi);
    }

    public BitmapSource ToDisplayBitmapSource()
    {
        if (_disposed) throw new InvalidOperationException();
        // WPF's high-quality scaling is very blurry for bitmaps whose DPI is far from 96 (e.g. 300 DPI scans)
        return _buffer.ToBitmapSource(96, 96);
    }

    public int Width => _buffer.Width;

    public int Height => _buffer.Height;

    public float HorizontalResolution => (float) _xDpi;

    public float VerticalResolution => (float) _yDpi;

    public void SetResolution(float xDpi, float yDpi)
    {
        _xDpi = xDpi;
        _yDpi = yDpi;
    }

    public ImagePixelFormat PixelFormat => _buffer.Format switch
    {
        { BitsPerPixel: 1 } => ImagePixelFormat.BW1,
        { BitsPerPixel: 8 } => ImagePixelFormat.Gray8,
        { BitsPerPixel: 24 } => ImagePixelFormat.RGB24,
        { BitsPerPixel: 32 } => ImagePixelFormat.ARGB32,
        _ => throw new InvalidOperationException("Unsupported pixel format")
    };

    public unsafe ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        if (_disposed) throw new InvalidOperationException();
        if (lockMode != LockMode.ReadOnly)
        {
            LogicalPixelFormat = ImagePixelFormat.Unknown;
        }
        var subPixelType = GetSubPixelType();
        // Hold a reference so the memory isn't freed if the image is disposed while locked
        bool addedRef = false;
        _buffer.DangerousAddRef(ref addedRef);
        imageData = new BitwiseImageData((byte*) _buffer.Ptr,
            new PixelInfo(Width, Height, subPixelType, _buffer.Stride));
        return new WpfImageLockState(_buffer);
    }

    private class WpfImageLockState(WpfPixelBuffer buffer) : ImageLockState
    {
        private bool _disposed;

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            buffer.DangerousRelease();
        }
    }

    private SubPixelType GetSubPixelType() => PixelFormat switch
    {
        ImagePixelFormat.RGB24 => SubPixelType.Bgr,
        ImagePixelFormat.ARGB32 => SubPixelType.Bgra,
        ImagePixelFormat.Gray8 => SubPixelType.Gray,
        ImagePixelFormat.BW1 => SubPixelType.Bit,
        _ => throw new InvalidOperationException("Unsupported pixel format")
    };

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unknown,
        ImageSaveOptions? options = null)
    {
        if (_disposed) throw new InvalidOperationException();
        if (imageFormat == ImageFileFormat.Unknown)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        if (ImageContext.TryBundledEncode(this, path, imageFormat, options))
        {
            return;
        }

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        encoder.Frames.Add(BitmapFrame.Create(helper.Image.ToBitmapSource()));
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        encoder.Save(stream);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (_disposed) throw new InvalidOperationException();
        if (imageFormat == ImageFileFormat.Unknown)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);
        if (ImageContext.TryBundledEncode(this, stream, imageFormat, options))
        {
            return;
        }

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        encoder.Frames.Add(BitmapFrame.Create(helper.Image.ToBitmapSource()));
        SaveMaybeWithoutSeeking(stream, encoder);
    }

    private static void SaveMaybeWithoutSeeking(Stream stream, BitmapEncoder encoder)
    {
        if (stream.CanSeek)
        {
            encoder.Save(stream);
        }
        else
        {
            var memoryStream = new MemoryStream();
            encoder.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.CopyTo(stream);
        }
    }

    private static BitmapEncoder GetImageEncoder(ImageFileFormat imageFormat, ImageSaveOptions options)
    {
        var encoder = imageFormat switch
        {
            ImageFileFormat.Bmp => (BitmapEncoder) new BmpBitmapEncoder(),
            ImageFileFormat.Png => new PngBitmapEncoder(),
            ImageFileFormat.Jpeg => new JpegBitmapEncoder
            {
                QualityLevel = options.Quality == -1 ? 75 : options.Quality
            },
            ImageFileFormat.Tiff => new TiffBitmapEncoder(),
            _ => throw new InvalidOperationException()
        };
        return encoder;
    }

    public IMemoryImage Clone()
    {
        if (_disposed) throw new InvalidOperationException();
        return new WpfImage(_buffer.Clone(), _xDpi, _yDpi)
        {
            OriginalFileFormat = OriginalFileFormat,
            LogicalPixelFormat = LogicalPixelFormat
        };
    }

    public void Dispose()
    {
        _disposed = true;
        _buffer.Dispose();
        LeakTracer.StopTracking(this);
    }
}