using NAPS2.Images.Bitwise;
using NAPS2.Util;
using SkiaSharp;

namespace NAPS2.Images.Skia;

public class SkiaImage : IMemoryImage
{
    private readonly SKBitmap _bitmap;
    private bool _disposed;

    public SkiaImage(SKBitmap bitmap)
    {
        LeakTracer.StartTracking(this);
        _bitmap = NormalizePixelFormat(bitmap);
    }

    private static SKBitmap NormalizePixelFormat(SKBitmap bitmap)
    {
        var info = bitmap.Info;
        var (colorType, alphaType) = (info.ColorType, info.AlphaType) switch
        {
            (SKColorType.Gray8, _) => (SKColorType.Gray8, SKAlphaType.Opaque),
            (_, SKAlphaType.Opaque) => (SKColorType.Rgb888x, SKAlphaType.Opaque),
            _ => (SKColorType.Rgba8888, SKAlphaType.Unpremul)
        };
        if (info.ColorType == colorType && info.AlphaType == alphaType)
        {
            return bitmap;
        }
        var newBitmap = new SKBitmap(new SKImageInfo(info.Width, info.Height, colorType, alphaType));
        try
        {
            using var pixmap = bitmap.PeekPixels();
            if (!pixmap.ReadPixels(newBitmap.Info, newBitmap.GetPixels(), newBitmap.RowBytes))
            {
                throw new InvalidOperationException("Could not convert Skia bitmap to a supported pixel format");
            }
        }
        catch
        {
            newBitmap.Dispose();
            throw;
        }
        bitmap.Dispose();
        return newBitmap;
    }

    public ImageContext ImageContext { get; } = new SkiaImageContext();

    public SKBitmap Bitmap => _disposed ? throw new ObjectDisposedException(nameof(SkiaImage)) : _bitmap;

    public int Width => Bitmap.Width;

    public int Height => Bitmap.Height;

    public float HorizontalResolution { get; private set; }

    public float VerticalResolution { get; private set; }

    public void SetResolution(float xDpi, float yDpi)
    {
        HorizontalResolution = xDpi;
        VerticalResolution = yDpi;
    }

    public ImagePixelFormat PixelFormat => Bitmap.ColorType switch
    {
        SKColorType.Gray8 => ImagePixelFormat.Gray8,
        SKColorType.Rgb888x => ImagePixelFormat.RGB24,
        SKColorType.Rgba8888 => ImagePixelFormat.ARGB32,
        _ => throw new InvalidOperationException("Unsupported pixel format")
    };

    public ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        if (lockMode != LockMode.ReadOnly)
        {
            LogicalPixelFormat = ImagePixelFormat.Unknown;
        }
        var subPixelType = PixelFormat switch
        {
            ImagePixelFormat.RGB24 => SubPixelType.Rgbn,
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            ImagePixelFormat.Gray8 => SubPixelType.Gray,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData(Bitmap.GetPixels(),
            new PixelInfo(Width, Height, subPixelType, Bitmap.RowBytes));
        return new SkiaImageLockState(Bitmap, lockMode);
    }

    internal class SkiaImageLockState : ImageLockState
    {
        private readonly SKBitmap _bitmap;
        private readonly LockMode _lockMode;

        public SkiaImageLockState(SKBitmap bitmap, LockMode lockMode)
        {
            _bitmap = bitmap;
            _lockMode = lockMode;
        }

        public override void Dispose()
        {
            if (_lockMode != LockMode.ReadOnly)
            {
                _bitmap.NotifyPixelsChanged();
            }
        }
    }

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unknown,
        ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unknown)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsEncode(imageFormat);
        if (ImageContext.TryBundledEncode(this, path, imageFormat, options))
        {
            return;
        }
        using var data = Encode(imageFormat, options);
        using var stream = File.Create(path);
        data.SaveTo(stream);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unknown)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsEncode(imageFormat);
        if (ImageContext.TryBundledEncode(this, stream, imageFormat, options))
        {
            return;
        }
        using var data = Encode(imageFormat, options);
        data.SaveTo(stream);
    }

    private SKData Encode(ImageFileFormat imageFormat, ImageSaveOptions? options)
    {
        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        using var pixmap = helper.Image.Bitmap.PeekPixels();
        var data = imageFormat switch
        {
            ImageFileFormat.Jpeg => EncodeJpeg(pixmap, options.Quality == -1 ? 75 : options.Quality),
            ImageFileFormat.Png => pixmap.Encode(SKPngEncoderOptions.Default),
            _ => throw new InvalidOperationException("Unsupported file format")
        };
        return data ?? throw new InvalidOperationException($"Skia could not encode the image as {imageFormat}");
    }

    private static SKData? EncodeJpeg(SKPixmap pixmap, int quality)
    {
        var encoderOptions = new SKJpegEncoderOptions(quality);
        if (pixmap.ColorType == SKColorType.Rgb888x)
        {
            // Skia's JPEG encoder doesn't support Rgb888x, but opaque Rgba8888 has the same memory layout
            using var rgbaPixmap = new SKPixmap(
                pixmap.Info.WithColorType(SKColorType.Rgba8888), pixmap.GetPixels(), pixmap.RowBytes);
            return rgbaPixmap.Encode(encoderOptions);
        }
        return pixmap.Encode(encoderOptions);
    }

    public IMemoryImage Clone() => new SkiaImage(Bitmap.Copy())
    {
        OriginalFileFormat = OriginalFileFormat,
        LogicalPixelFormat = LogicalPixelFormat,
        HorizontalResolution = HorizontalResolution,
        VerticalResolution = VerticalResolution
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _bitmap.Dispose();
        LeakTracer.StopTracking(this);
    }
}
