using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace NAPS2.Images.Gdi;

#if NET6_0_OR_GREATER
[System.Runtime.Versioning.SupportedOSPlatform("windows7.0")]
#endif
public class GdiImageContext : ImageContext
{
    private readonly GdiImageTransformer _imageTransformer;

    public GdiImageContext() : this(null)
    {
    }

    public GdiImageContext(IPdfRenderer? pdfRenderer) : base(typeof(GdiImage), pdfRenderer)
    {
        _imageTransformer = new GdiImageTransformer(this);
        LoadFromFileKeepsLock = true;
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GdiImage ?? throw new ArgumentException("Expected GdiImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path)
    {
        var format = GetFileFormatFromExtension(path, true);
        var image = new GdiImage(this, LoadBitmapWithExceptionHandling(path));
        if (format != ImageFileFormat.Unspecified)
        {
            image.OriginalFileFormat = format;
        }
        return image;
    }

    public override IMemoryImage Load(Stream stream)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        var image = new GdiImage(this, new Bitmap(stream));
        if (format != ImageFileFormat.Unspecified)
        {
            image.OriginalFileFormat = format;
        }
        return image;
    }

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        var bitmap = new Bitmap(stream);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, format, count);
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var format = GetFileFormatFromExtension(path);
        var bitmap = LoadBitmapWithExceptionHandling(path);
        count = bitmap.GetFrameCount(FrameDimension.Page);
        return EnumerateFrames(bitmap, format, count);
    }

    private static Bitmap LoadBitmapWithExceptionHandling(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch (ArgumentException)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Could not find image file '{path}'.");
            }
            throw new IOException($"Error reading image file '{path}'.");
        }
    }

    private IEnumerable<IMemoryImage> EnumerateFrames(Bitmap bitmap, ImageFileFormat format, int count)
    {
        using (bitmap)
        {
            for (int i = 0; i < count; i++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Page, i);
                var image = new GdiImage(this, (Bitmap) bitmap.Clone());
                if (format != ImageFileFormat.Unspecified)
                {
                    image.OriginalFileFormat = format;
                }
                yield return image;
            }
        }
    }

    public override bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default)
    {
        return SaveTiffInternal(images,
            (bitmap, codecInfo, encoderParams) => bitmap.Save(path, codecInfo, encoderParams),
            () => File.Delete(path), compression, progressCallback, cancelToken);
    }

    public override bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default)
    {
        return SaveTiffInternal(images,
            (bitmap, codecInfo, encoderParams) => bitmap.Save(stream, codecInfo, encoderParams),
            () => { }, compression, progressCallback, cancelToken);
    }

    private bool SaveTiffInternal(IList<IMemoryImage> images, Action<Bitmap, ImageCodecInfo, EncoderParameters> save,
        Action cleanup, TiffCompressionType compression, Action<int, int>? progressCallback, CancellationToken cancelToken)
    {
        ImageCodecInfo codecInfo = GetCodecForString("TIFF");

        progressCallback?.Invoke(0, images.Count);
        if (cancelToken.IsCancellationRequested)
        {
            return false;
        }

        using var image0 = (GdiImage) images[0].Clone();
        if (images.Count == 1)
        {
            save(image0.Bitmap, codecInfo, GetTiffParameters(compression, image0));
        }
        else if (images.Count > 1)
        {
            save(image0.Bitmap, codecInfo,
                GetTiffParameters(compression, images[0], Encoder.SaveFlag, EncoderValue.MultiFrame));

            for (int i = 1; i < images.Count; i++)
            {
                progressCallback?.Invoke(i, images.Count);
                if (cancelToken.IsCancellationRequested)
                {
                    cleanup();
                    return false;
                }

                using var image = (GdiImage) images[i].Clone();
                image0.Bitmap.SaveAdd(image.Bitmap,
                    GetTiffParameters(compression, images[0], Encoder.SaveFlag, EncoderValue.FrameDimensionPage));
            }

            image0.Bitmap.SaveAdd(new EncoderParameters(1)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.SaveFlag, (long) EncoderValue.Flush)
                }
            });
        }
        return true;
    }

    private EncoderParameters GetTiffParameters(TiffCompressionType compression, IMemoryImage image,
        Encoder? secondParam = null, EncoderValue? secondValue = null) =>
        secondParam != null && secondValue != null
            ? new(2)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.Compression,
                        (long) GetTiffCompressionValue(compression, image)),
                    [1] = new EncoderParameter(secondParam, (long) secondValue.Value)
                }
            }
            : new(1)
            {
                Param =
                {
                    [0] = new EncoderParameter(Encoder.Compression,
                        (long) GetTiffCompressionValue(compression, image))
                }
            };

    private EncoderValue GetTiffCompressionValue(TiffCompressionType compression, IMemoryImage image) =>
        compression switch
        {
            TiffCompressionType.None => EncoderValue.CompressionNone,
            TiffCompressionType.Ccitt4 => EncoderValue.CompressionCCITT4,
            TiffCompressionType.Lzw => EncoderValue.CompressionLZW,
            _ => image.PixelFormat == ImagePixelFormat.BW1
                ? EncoderValue.CompressionCCITT4
                : EncoderValue.CompressionLZW
        };

    private ImageCodecInfo GetCodecForString(string type)
    {
        ImageCodecInfo[] info = ImageCodecInfo.GetImageEncoders();
        return info.First(t => t.FormatDescription == type);
    }

    public Bitmap RenderToBitmap(IRenderableImage image)
    {
        return ((GdiImage) Render(image)).Bitmap;
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        var bitmap = new Bitmap(width, height, pixelFormat.AsPixelFormat());
        if (pixelFormat == ImagePixelFormat.BW1)
        {
            var p = bitmap.Palette;
            p.Entries[0] = Color.Black;
            p.Entries[1] = Color.White;
            bitmap.Palette = p;
        }
        if (pixelFormat == ImagePixelFormat.Gray8)
        {
            var p = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                p.Entries[i] = Color.FromArgb(i, i, i);
            }
            bitmap.Palette = p;
        }
        return new GdiImage(this, bitmap);
    }
}