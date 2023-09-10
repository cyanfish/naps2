using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAPS2.Util;
using Transform = NAPS2.Images.Transforms.Transform;

namespace NAPS2.Images.Wpf;

public class WpfImageContext : ImageContext
{
    private readonly WpfImageTransformer _imageTransformer;

    public WpfImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(WpfImage), pdfRenderer)
    {
        _imageTransformer = new WpfImageTransformer(this);
    }

    protected override bool SupportsTiff => true;

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var wpfImage = image as WpfImage ?? throw new ArgumentException("Expected WpfImage object");
        return _imageTransformer.Apply(wpfImage, transform);
    }

    protected override IMemoryImage LoadCore(Stream stream, ImageFileFormat format)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return new WpfImage(this, new WriteableBitmap(bitmap));
    }

    protected override void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress)
    {
        if (format == ImageFileFormat.Tiff)
        {
            var decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
            progress.Report(0, decoder.Frames.Count);
            int i = 0;
            foreach (var frame in decoder.Frames)
            {
                if (progress.IsCancellationRequested) return;
                produceImage(new WpfImage(this, new WriteableBitmap(frame)));
                progress.Report(++i, decoder.Frames.Count);
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
        return ((WpfImage) Render(image)).Bitmap;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var wpfPixelFormat = pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => PixelFormats.Bgr32,
            ImagePixelFormat.RGB24 => PixelFormats.Bgr24,
            ImagePixelFormat.Gray8 => PixelFormats.Gray8,
            ImagePixelFormat.BW1 => PixelFormats.BlackWhite,
            _ => throw new InvalidOperationException("Unsupported pixel format")
        };
        var image = new WriteableBitmap(width, height, 0, 0, wpfPixelFormat, null);
        return new WpfImage(this, image);
    }
}