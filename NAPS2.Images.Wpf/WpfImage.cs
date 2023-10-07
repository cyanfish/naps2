using System.Windows.Media.Imaging;
using NAPS2.Images.Bitwise;
using NAPS2.Util;

namespace NAPS2.Images.Wpf;

public class WpfImage : IMemoryImage
{
    private bool _disposed;

    public WpfImage(ImageContext imageContext, WriteableBitmap bitmap)
    {
        if (imageContext is not WpfImageContext) throw new ArgumentException("Expected WpfImageContext");
        LeakTracer.StartTracking(this);
        ImageContext = imageContext;
        // TODO: Something similar to MacImage where if it's not a supported pixel type we convert
        WpfPixelFormatFixer.MaybeFixPixelFormat(ref bitmap);
        Bitmap = bitmap;
        LogicalPixelFormat = PixelFormat;
        Bitmap.Freeze();
    }

    public ImageContext ImageContext { get; }

    public WriteableBitmap Bitmap { get; private set; }

    public int Width => Bitmap.PixelWidth;

    public int Height => Bitmap.PixelHeight;

    public float HorizontalResolution => (float) Bitmap.DpiX;

    public float VerticalResolution => (float) Bitmap.DpiY;

    public void SetResolution(float xDpi, float yDpi)
    {
        var src = Bitmap.BackBuffer;
        var srcInfo = new PixelInfo(Width, Height, GetSubPixelType(), Bitmap.BackBufferStride);

        var newImage = new WriteableBitmap(Width, Height, xDpi, yDpi, Bitmap.Format, null);
        var dst = newImage.BackBuffer;
        var dstInfo = new PixelInfo(Width, Height, GetSubPixelType(), newImage.BackBufferStride);

        new CopyBitwiseImageOp().Perform(src, srcInfo, dst, dstInfo);
        newImage.Freeze();
        Bitmap = newImage;
    }

    public ImagePixelFormat PixelFormat => Bitmap.Format switch
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
        var subPixelType = GetSubPixelType();
        if (lockMode == LockMode.ReadOnly)
        {
            imageData = new BitwiseImageData((byte*) Bitmap.BackBuffer,
                new PixelInfo(Width, Height, subPixelType, Bitmap.BackBufferStride));
            return new WpfReadOnlyImageLockState();
        }
        else
        {
            var copy = new WriteableBitmap(Bitmap);
            imageData = new BitwiseImageData((byte*) copy.BackBuffer,
                new PixelInfo(Width, Height, subPixelType, copy.BackBufferStride));
            return new WpfWritableImageLockState(copy, this);
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

    private class WpfReadOnlyImageLockState : ImageLockState
    {
        public WpfReadOnlyImageLockState()
        {
        }

        public override void Dispose()
        {
        }
    }

    private class WpfWritableImageLockState : ImageLockState
    {
        private readonly WpfImage _container;
        private readonly WriteableBitmap _copy;

        public WpfWritableImageLockState(WriteableBitmap copy, WpfImage container)
        {
            _container = container;
            _copy = copy;
            _copy.Lock();
        }

        public override void Dispose()
        {
            if (!_copy.IsFrozen)
            {
                _copy.Unlock();
                _copy.Freeze();
                _container.Bitmap = _copy;
            }
        }
    }

    public ImageFileFormat OriginalFileFormat { get; set; }

    public ImagePixelFormat LogicalPixelFormat { get; set; }

    public void Save(string path, ImageFileFormat imageFormat = ImageFileFormat.Unspecified,
        ImageSaveOptions? options = null)
    {
        if (_disposed) throw new InvalidOperationException();
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            imageFormat = ImageContext.GetFileFormatFromExtension(path);
        }
        ImageContext.CheckSupportsFormat(imageFormat);

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        encoder.Frames.Add(BitmapFrame.Create(helper.Image.Bitmap));
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        encoder.Save(stream);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (_disposed) throw new InvalidOperationException();
        if (imageFormat == ImageFileFormat.Unspecified)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        encoder.Frames.Add(BitmapFrame.Create(helper.Image.Bitmap));
        encoder.Save(stream);
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
        return new WpfImage(ImageContext, Bitmap.Clone())
        {
            OriginalFileFormat = OriginalFileFormat,
            LogicalPixelFormat = LogicalPixelFormat
        };
    }

    public void Dispose()
    {
        _disposed = true;
        LeakTracer.StopTracking(this);
    }
}