using NAPS2.Util;
using SkiaSharp;

namespace NAPS2.Images.Skia;

public class SkiaImageContext : ImageContext
{
    private readonly SkiaImageTransformer _imageTransformer;

    public SkiaImageContext() : base(typeof(SkiaImage))
    {
        _imageTransformer = new SkiaImageTransformer(this);
    }

    protected override bool SupportsBmpEncode => false;

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var skiaImage = image as SkiaImage ?? throw new ArgumentException("Expected SkiaImage object");
        return _imageTransformer.Apply(skiaImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        using var data = SKData.Create(stream);
        using var codec = SKCodec.Create(data) ?? throw new IOException("Could not decode image");
        var codecInfo = codec.Info;

        // Skia codecs can't decode directly to Rgb888x, but opaque Rgba8888 has the same memory layout
        var (bitmapColorType, decodeColorType, alphaType) = (codecInfo.ColorType, codecInfo.AlphaType) switch
        {
            (SKColorType.Gray8, _) => (SKColorType.Gray8, SKColorType.Gray8, SKAlphaType.Opaque),
            (_, SKAlphaType.Opaque) => (SKColorType.Rgb888x, SKColorType.Rgba8888, SKAlphaType.Opaque),
            _ => (SKColorType.Rgba8888, SKColorType.Rgba8888, SKAlphaType.Unpremul)
        };
        var bitmap = new SKBitmap(new SKImageInfo(codecInfo.Width, codecInfo.Height, bitmapColorType, alphaType));
        try
        {
            var decodeInfo = new SKImageInfo(codecInfo.Width, codecInfo.Height, decodeColorType, alphaType);
            var result = codec.GetPixels(decodeInfo, bitmap.GetPixels());
            if (result is not (SKCodecResult.Success or SKCodecResult.IncompleteInput))
            {
                throw new IOException($"Could not decode image: {result}");
            }
            return new SkiaImage(bitmap);
        }
        catch
        {
            bitmap.Dispose();
            throw;
        }
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        progress.Report(0, 1);
        if (progress.IsCancellationRequested) return;
        produceImage(LoadCore(stream, format));
        progress.Report(1, 1);
    }

    public SKBitmap RenderToBitmap(IRenderableImage image)
    {
        return ((SkiaImage) Render(image)).Bitmap;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        var info = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul),
            ImagePixelFormat.RGB24 => new SKImageInfo(width, height, SKColorType.Rgb888x, SKAlphaType.Opaque),
            ImagePixelFormat.Gray8 or ImagePixelFormat.BW1 =>
                new SKImageInfo(width, height, SKColorType.Gray8, SKAlphaType.Opaque),
            _ => throw new ArgumentException("Unsupported pixel format")
        };
        return new SkiaImage(new SKBitmap(info));
    }
}
