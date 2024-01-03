using NAPS2.Util;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace NAPS2.Images.ImageSharp;

public class ImageSharpImageContext : ImageContext
{
    private readonly ImageSharpImageTransformer _imageTransformer;

    public static Configuration GetConfiguration()
    {
        var config = Configuration.Default.Clone();
        config.PreferContiguousImageBuffers = true;
        return config;
    }

    public static DecoderOptions GetDecoderOptions() => new()
    {
        Configuration = GetConfiguration()
    };

    public ImageSharpImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(ImageSharpImage), pdfRenderer)
    {
        _imageTransformer = new ImageSharpImageTransformer(this);
    }

    protected override bool SupportsTiff => true;

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var imageSharpImage = image as ImageSharpImage ?? throw new ArgumentException("Expected ImageSharpImage object");
        return _imageTransformer.Apply(imageSharpImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        return new ImageSharpImage(this, Image.Load(GetDecoderOptions(), stream));
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        progress.Report(0, 1);
        if (progress.IsCancellationRequested) return;
        produceImage(LoadCore(stream, format));
        progress.Report(1, 1);
    }

    public Image RenderToImage(IRenderableImage image)
    {
        return ((ImageSharpImage) Render(image)).Image;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unknown)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var image = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => (Image) new Image<Rgba32>(GetConfiguration(), width, height),
            ImagePixelFormat.RGB24 => new Image<Rgb24>(GetConfiguration(), width, height),
            ImagePixelFormat.Gray8 or ImagePixelFormat.BW1 => new Image<L8>(GetConfiguration(), width, height),
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        return new ImageSharpImage(this, image);
    }
}