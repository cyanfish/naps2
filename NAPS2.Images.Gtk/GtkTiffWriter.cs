using System.Threading;
using NAPS2.Images.Bitwise;

namespace NAPS2.Images.Gtk;

internal class LibTiffIo : ITiffWriter
{
    private readonly ImageContext _imageContext;

    public LibTiffIo(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto,
        Action<int, int>? progressCallback = null, CancellationToken cancelToken = default)
    {
        var tiff = LibTiff.TIFFOpen(path, "w");
        return WriteTiff(tiff, null, images, compression, progressCallback, cancelToken);
    }

    public bool SaveTiff(IList<IMemoryImage> images, Stream stream,
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
        // TODO: A lot of these types are wrong (e.g. int32 instead of int16)
        // http://www.libtiff.org/man/TIFFSetField.3t.html
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
            // TODO: I think this is completely wrong
            LibTiff.TIFFSetField(tiff, TiffTag.ExtraSamples, 1);
        }
        if (image.HorizontalResolution != 0 && image.VerticalResolution != 0)
        {
            LibTiff.TIFFSetField(tiff, TiffTag.ResolutionUnit, 2);
            // TODO: Why do we need to write as a double? It's supposed to be a float.
            LibTiff.TIFFSetField(tiff, TiffTag.XResolution, (double) image.HorizontalResolution);
            LibTiff.TIFFSetField(tiff, TiffTag.YResolution, (double) image.VerticalResolution);
        }
    }

    public IEnumerable<IMemoryImage> LoadTiff(Stream stream, out int count)
    {
        var client = new LibTiffStreamClient(stream);
        var tiff = client.TIFFClientOpen("r");
        count = LibTiff.TIFFNumberOfDirectories(tiff);
        return EnumerateTiffFrames(tiff, client);
    }

    public IEnumerable<IMemoryImage> LoadTiff(string path, out int count)
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
                LibTiff.TIFFGetField(tiff, TiffTag.ImageWidth, out int w);
                LibTiff.TIFFGetField(tiff, TiffTag.ImageHeight, out int h);
                // TODO: Check return values
                LibTiff.TIFFGetField(tiff, TiffTag.XResolution, out float xres);
                LibTiff.TIFFGetField(tiff, TiffTag.YResolution, out float yres);
                var img = _imageContext.Create(w, h, ImagePixelFormat.ARGB32);
                img.SetResolution(xres, yres);
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
}