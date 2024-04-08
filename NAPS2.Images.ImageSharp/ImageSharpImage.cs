using System.Buffers;
using NAPS2.Images.Bitwise;
using NAPS2.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NAPS2.Images.ImageSharp;

public class ImageSharpImage : IMemoryImage
{
    public ImageSharpImage(Image image)
    {
        LeakTracer.StartTracking(this);
        // TODO: Something similar to MacImage where if it's not a supported pixel type we convert
        // TODO: Though we might also want to add support where reasonable, e.g. we can probably support argb or bgr pretty easily?
        Image = image;
    }

    public ImageContext ImageContext { get; } = new ImageSharpImageContext();

    public Image Image { get; }

    public int Width => Image.Width;

    public int Height => Image.Height;

    public float HorizontalResolution => (float) (Image.Metadata.HorizontalResolution * ResolutionMultiplier);

    public float VerticalResolution => (float) (Image.Metadata.HorizontalResolution * ResolutionMultiplier);

    private double ResolutionMultiplier => Image.Metadata.ResolutionUnits switch
    {
        PixelResolutionUnit.PixelsPerCentimeter => 2.54,
        PixelResolutionUnit.PixelsPerMeter => 2.54 / 100,
        _ => 1
    };

    public void SetResolution(float xDpi, float yDpi)
    {
        Image.Metadata.ResolutionUnits = PixelResolutionUnit.PixelsPerInch;
        Image.Metadata.HorizontalResolution = xDpi;
        Image.Metadata.VerticalResolution = yDpi;
    }

    public ImagePixelFormat PixelFormat => Image.PixelType switch
    {
        { BitsPerPixel: 8 } => ImagePixelFormat.Gray8,
        { BitsPerPixel: 24 } => ImagePixelFormat.RGB24,
        { BitsPerPixel: 32 } => ImagePixelFormat.ARGB32,
        _ => throw new InvalidOperationException("Unsupported pixel format")
    };

    public unsafe ImageLockState Lock(LockMode lockMode, out BitwiseImageData imageData)
    {
        if (lockMode != LockMode.ReadOnly)
        {
            LogicalPixelFormat = ImagePixelFormat.Unknown;
        }
        var memoryHandle = PixelFormat switch
        {
            ImagePixelFormat.RGB24 => ((Image<Rgb24>) Image).DangerousTryGetSinglePixelMemory(out var mem)
                ? mem.Pin()
                : throw new InvalidOperationException("Could not get contiguous memory for ImageSharp image"),
            ImagePixelFormat.ARGB32 => ((Image<Rgba32>) Image).DangerousTryGetSinglePixelMemory(out var mem)
                ? mem.Pin()
                : throw new InvalidOperationException("Could not get contiguous memory for ImageSharp image"),
            ImagePixelFormat.Gray8 => ((Image<L8>) Image).DangerousTryGetSinglePixelMemory(out var mem)
                ? mem.Pin()
                : throw new InvalidOperationException("Could not get contiguous memory for ImageSharp image"),
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        var subPixelType = PixelFormat switch
        {
            ImagePixelFormat.RGB24 => SubPixelType.Rgb,
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            ImagePixelFormat.Gray8 => SubPixelType.Gray,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        imageData = new BitwiseImageData((byte*) memoryHandle.Pointer, new PixelInfo(Width, Height, subPixelType));
        return new ImageSharpImageLockState(memoryHandle);
    }

    internal class ImageSharpImageLockState : ImageLockState
    {
        private readonly MemoryHandle _memoryHandle;

        public ImageSharpImageLockState(MemoryHandle memoryHandle)
        {
            _memoryHandle = memoryHandle;
        }

        public override void Dispose()
        {
            _memoryHandle.Dispose();
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
        ImageContext.CheckSupportsFormat(imageFormat);

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        helper.Image.Image.Save(path, encoder);
    }

    public void Save(Stream stream, ImageFileFormat imageFormat, ImageSaveOptions? options = null)
    {
        if (imageFormat == ImageFileFormat.Unknown)
        {
            throw new ArgumentException("Format required to save to a stream", nameof(imageFormat));
        }
        ImageContext.CheckSupportsFormat(imageFormat);

        options ??= new ImageSaveOptions();
        using var helper = PixelFormatHelper.Create(this, options.PixelFormatHint, minFormat: ImagePixelFormat.Gray8);
        var encoder = GetImageEncoder(imageFormat, options);
        helper.Image.Image.Save(stream, encoder);
    }

    private static ImageEncoder GetImageEncoder(ImageFileFormat imageFormat, ImageSaveOptions options)
    {
        var encoder = imageFormat switch
        {
            ImageFileFormat.Bmp => (ImageEncoder) new BmpEncoder(),
            ImageFileFormat.Png => new PngEncoder(),
            ImageFileFormat.Jpeg => new JpegEncoder
            {
                Quality = options.Quality == -1 ? 75 : options.Quality,
                // ImageSharp will automatically save an RGB24 image as Grayscale if the actual image colors are gray.
                // We prevent that here if the caller specified an RGB PixelFormatHint.
                ColorType = options.PixelFormatHint >= ImagePixelFormat.RGB24 ? JpegEncodingColor.Rgb : null
            },
            ImageFileFormat.Tiff => new TiffEncoder(),
            _ => throw new InvalidOperationException()
        };
        return encoder;
    }

    public IMemoryImage Clone() => new ImageSharpImage(Image.Clone(_ => { }))
    {
        OriginalFileFormat = OriginalFileFormat,
        LogicalPixelFormat = LogicalPixelFormat
    };

    public void Dispose()
    {
        Image.Dispose();
        LeakTracer.StopTracking(this);
    }
}