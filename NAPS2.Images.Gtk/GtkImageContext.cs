using System.Threading;
using Gdk;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gtk;

public class GtkImageContext : ImageContext
{
    private readonly GtkImageTransformer _imageTransformer;

    public GtkImageContext(IPdfRenderer? pdfRenderer = null) : base(typeof(GtkImage), pdfRenderer)
    {
        _imageTransformer = new GtkImageTransformer(this);
    }

    public override IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        var gdiImage = image as GtkImage ?? throw new ArgumentException("Expected GtkImage object");
        return _imageTransformer.Apply(gdiImage, transform);
    }

    public override IMemoryImage Load(string path)
    {
        using var readStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var pixbuf = new Pixbuf(readStream);
        return new GtkImage(this, pixbuf, pixbuf.HasAlpha ? ImagePixelFormat.ARGB32 : ImagePixelFormat.RGB24)
        {
            OriginalFileFormat = GetFileFormatFromExtension(path, true)
        };
    }

    public override IMemoryImage Load(Stream stream)
    {
        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }
        var pixbuf = new Pixbuf(stream);
        return new GtkImage(this, pixbuf, pixbuf.HasAlpha ? ImagePixelFormat.ARGB32 : ImagePixelFormat.RGB24)
        {
            OriginalFileFormat = GetFileFormatFromFirstBytes(stream)
        };
    }

    public override IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        if (format == ImageFileFormat.Tiff)
        {
            return LoadTiff(stream, out count);
        }
        count = 1;
        return new[] { Load(stream) };
    }

    public override IEnumerable<IMemoryImage> LoadFrames(string path, out int count)
    {
        var format = GetFileFormatFromExtension(path, true);
        if (format == ImageFileFormat.Tiff)
        {
            return LoadTiff(path, out count);
        }
        count = 1;
        return new[] { Load(path) };
    }

    public override bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto,
        Action<int, int>? progressCallback = null, CancellationToken cancelToken = default)
    {
        var tiff = LibTiff.TIFFOpen(path, "w");
        return WriteTiff(tiff, null, images, compression, progressCallback, cancelToken);
    }

    public override bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto,
        Action<int, int>? progressCallback = null, CancellationToken cancelToken = default)
    {
        var client = new LibTiffStreamClient(stream);
        var tiff = client.TIFFClientOpen("w");
        return WriteTiff(tiff, client, images, compression, progressCallback, cancelToken);
    }

    private bool WriteTiff(IntPtr tiff, LibTiffStreamClient client, IList<IMemoryImage> images,
        TiffCompressionType compression, Action<int, int>? progressCallback, CancellationToken cancelToken)
    {
        try
        {
            int i = 0;
            progressCallback?.Invoke(0, images.Count);
            foreach (var image in images)
            {
                if (cancelToken.IsCancellationRequested) return false;
                var pixelFormat =
                    image.LogicalPixelFormat == ImagePixelFormat.BW1 || compression == TiffCompressionType.Ccitt4
                        ? ImagePixelFormat.BW1
                        : image.LogicalPixelFormat;
                WriteTiffMetadata(tiff, pixelFormat, compression, image);
                WriteTiffImageData(tiff, pixelFormat, image);
                if (images.Count > 1)
                {
                    LibTiff.TIFFWriteDirectory(tiff);
                }
                progressCallback?.Invoke(++i, images.Count);
            }
            return true;
        }
        finally
        {
            LibTiff.TIFFClose(tiff);
        }
    }

    private unsafe void WriteTiffImageData(IntPtr tiff, ImagePixelFormat pixelFormat, IMemoryImage image)
    {
        var bufferInfo = new PixelInfo(image.Width, image.Height, pixelFormat switch
        {
            ImagePixelFormat.ARGB32 => SubPixelType.Rgba,
            ImagePixelFormat.RGB24 => SubPixelType.Rgb,
            ImagePixelFormat.Gray8 => SubPixelType.Gray,
            ImagePixelFormat.BW1 => SubPixelType.Bit
        });
        var buffer = new byte[bufferInfo.Length];
        new CopyBitwiseImageOp().Perform(image, buffer, bufferInfo);
        fixed (byte* buf = buffer)
        {
            for (int i = 0; i < image.Height; i++)
            {
                LibTiff.TIFFWriteScanline(tiff, (IntPtr) (buf + bufferInfo.Stride * i), i, 0);
            }
        }
    }

    private static void WriteTiffMetadata(IntPtr tiff, ImagePixelFormat pixelFormat,
        TiffCompressionType compression, IMemoryImage image)
    {
        LibTiff.TIFFSetField(tiff, TiffTag.ImageWidth, image.Width);
        LibTiff.TIFFSetField(tiff, TiffTag.ImageHeight, image.Height);
        LibTiff.TIFFSetField(tiff, TiffTag.PlanarConfig, 1);
        // TODO: Test setting g4 compression when it's not a BW image
        LibTiff.TIFFSetField(tiff, TiffTag.Compression, (int) (compression switch
        {
            TiffCompressionType.Auto => pixelFormat == ImagePixelFormat.BW1
                ? TiffCompression.G4
                : TiffCompression.Lzw,
            TiffCompressionType.Ccitt4 => TiffCompression.G4,
            TiffCompressionType.Lzw => TiffCompression.Lzw,
            TiffCompressionType.None => TiffCompression.None
        }));
        LibTiff.TIFFSetField(tiff, TiffTag.Orientation, 1);
        LibTiff.TIFFSetField(tiff, TiffTag.BitsPerSample, pixelFormat == ImagePixelFormat.BW1 ? 1 : 8);
        LibTiff.TIFFSetField(tiff, TiffTag.SamplesPerPixel, pixelFormat switch
        {
            ImagePixelFormat.RGB24 => 3,
            ImagePixelFormat.ARGB32 => 4,
            _ => 1
        });
        LibTiff.TIFFSetField(tiff, TiffTag.Photometric, (int) (pixelFormat switch
        {
            ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32 => TiffPhotometric.Rgb,
            _ => TiffPhotometric.MinIsBlack
        }));
        if (pixelFormat == ImagePixelFormat.ARGB32)
        {
            LibTiff.TIFFSetField(tiff, TiffTag.ExtraSamples, 1);
        }
        if (image.HorizontalResolution != 0 && image.VerticalResolution != 0)
        {
            LibTiff.TIFFSetField(tiff, TiffTag.ResolutionUnit, 2);
            LibTiff.TIFFSetField(tiff, TiffTag.XResolution, (int) image.HorizontalResolution);
            LibTiff.TIFFSetField(tiff, TiffTag.YResolution, (int) image.VerticalResolution);
        }
    }

    private IEnumerable<IMemoryImage> LoadTiff(Stream stream, out int count)
    {
        var client = new LibTiffStreamClient(stream);
        var tiff = client.TIFFClientOpen("r");
        count = LibTiff.TIFFNumberOfDirectories(tiff);
        return EnumerateTiffFrames(tiff, client);
    }

    private IEnumerable<IMemoryImage> LoadTiff(string path, out int count)
    {
        var tiff = LibTiff.TIFFOpen(path, "r");
        count = LibTiff.TIFFNumberOfDirectories(tiff);
        return EnumerateTiffFrames(tiff);
    }

    private IEnumerable<IMemoryImage> EnumerateTiffFrames(IntPtr tiff, LibTiffStreamClient? client = null)
    {
        // We keep a reference to the client to avoid garbage collection
        try
        {
            do
            {
                LibTiff.TIFFGetField(tiff, TiffTag.ImageWidth, out var w);
                LibTiff.TIFFGetField(tiff, TiffTag.ImageHeight, out var h);
                var img = Create(w, h, ImagePixelFormat.ARGB32);
                img.OriginalFileFormat = ImageFileFormat.Tiff;
                using var imageLock = img.Lock(LockMode.WriteOnly, out var data);
                ReadTiffFrame(data.safePtr, tiff, w, h);
                yield return img;
            } while (LibTiff.TIFFReadDirectory(tiff) == 1);
        }
        finally
        {
            LibTiff.TIFFClose(tiff);
        }
    }

    private static void ReadTiffFrame(IntPtr buffer, IntPtr tiff, int w, int h)
    {
        int r = LibTiff.TIFFReadRGBAImageOriented(tiff, w, h, buffer, 1, 0);
        // TODO: Check return value
    }

    public override IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat)
    {
        if (pixelFormat == ImagePixelFormat.Unsupported)
        {
            throw new ArgumentException("Unsupported pixel format");
        }
        var pixbuf = new Pixbuf(Colorspace.Rgb, pixelFormat == ImagePixelFormat.ARGB32, 8, width, height);
        return new GtkImage(this, pixbuf, pixelFormat);
    }
}