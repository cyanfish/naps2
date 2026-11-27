using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAPS2.Util;
using Transform = NAPS2.Images.Transforms.Transform;

namespace NAPS2.Images.Wpf;

public class WpfImageContext : ImageContext
{
    private readonly WpfImageTransformer _imageTransformer;

    public WpfImageContext() : base(typeof(WpfImage))
    {
        _imageTransformer = new WpfImageTransformer(this);
    }

    protected override bool SupportsDecodeCore(ImageFileFormat format) =>
        format is ImageFileFormat.Jpeg or ImageFileFormat.Png or ImageFileFormat.Bmp or ImageFileFormat.Gif
            or ImageFileFormat.Tiff
            || format == ImageFileFormat.Heic && WicCodecSupport.Heic
            || format == ImageFileFormat.Avif && WicCodecSupport.Avif;

    protected override bool SupportsEncodeCore(ImageFileFormat format) =>
        format is ImageFileFormat.Jpeg or ImageFileFormat.Png or ImageFileFormat.Bmp or ImageFileFormat.Tiff;

    public override ITiffWriter TiffWriter => new WpfTiffWriter();

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var wpfImage = image as WpfImage ?? throw new ArgumentException("Expected WpfImage object");
        return _imageTransformer.Apply(wpfImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        var loader = new WpfFrameLoader(stream,
            (options, cacheOption) => BitmapDecoder.Create(stream, options, cacheOption));
        return loader.LoadFrame(0);
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        if (format == ImageFileFormat.Tiff)
        {
            var loader = new WpfFrameLoader(stream,
                (options, cacheOption) => new TiffBitmapDecoder(stream, options | BitmapCreateOptions.DelayCreation,
                    cacheOption));
            int count = loader.FrameCount;
            progress.Report(0, count);
            for (int i = 0; i < count; i++)
            {
                if (progress.IsCancellationRequested) return;
                produceImage(loader.LoadFrame(i));
                progress.Report(i + 1, count);
            }
            return;
        }
        progress.Report(0, 1);
        if (progress.IsCancellationRequested) return;
        produceImage(LoadCore(stream, format));
        progress.Report(1, 1);
    }

    public BitmapSource RenderToBitmapSource(IRenderableImage image)
    {
        using var rendered = (WpfImage) Render(image);
        return rendered.ToBitmapSource();
    }

    public BitmapSource RenderToDisplayBitmapSource(IRenderableImage image)
    {
        using var rendered = (WpfImage) Render(image);
        return rendered.ToDisplayBitmapSource();
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unknown)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var wpfPixelFormat = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => PixelFormats.Bgra32,
            ImagePixelFormat.RGB24 => PixelFormats.Bgr24,
            ImagePixelFormat.Gray8 => PixelFormats.Gray8,
            ImagePixelFormat.BW1 => PixelFormats.BlackWhite,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        return new WpfImage(WpfPixelBuffer.Allocate(width, height, wpfPixelFormat, true), 0, 0);
    }
}