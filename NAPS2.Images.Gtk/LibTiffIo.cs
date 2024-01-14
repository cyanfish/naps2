using System.Runtime.InteropServices;
using NAPS2.Images.Bitwise;
using NAPS2.Util;

namespace NAPS2.Images.Gtk;

internal class LibTiffIo : ITiffWriter
{
    private readonly ImageContext _imageContext;

    public LibTiffIo(ImageContext imageContext)
    {
        _imageContext = imageContext;
    }

    public bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        var tiff = LibTiff.TIFFOpen(path, "w");
        return WriteTiff(tiff, null, images, compression, progress);
    }

    public bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, ProgressHandler progress = default)
    {
        var client = new LibTiffStreamClient(stream);
        var tiff = client.TIFFClientOpen("w");
        return WriteTiff(tiff, client, images, compression, progress);
    }

    private bool WriteTiff(IntPtr tiff, LibTiffStreamClient? client, IList<IMemoryImage> images,
        TiffCompressionType compression, ProgressHandler progress = default)
    {
        try
        {
            int i = 0;
            progress.Report(0, images.Count);
            foreach (var image in images)
            {
                if (progress.IsCancellationRequested) return false;
                image.UpdateLogicalPixelFormat();
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
                progress.Report(++i, images.Count);
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
            ImagePixelFormat.BW1 => SubPixelType.Bit,
            _ => throw new InvalidOperationException("Unsupported pixel format")
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
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.ImageWidth, image.Width);
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.ImageHeight, image.Height);
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.PlanarConfig, 1);
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.Compression, (int) (compression switch
        {
            TiffCompressionType.Ccitt4 => TiffCompression.G4,
            TiffCompressionType.Lzw => TiffCompression.Lzw,
            TiffCompressionType.None => TiffCompression.None,
            _ => pixelFormat == ImagePixelFormat.BW1
                ? TiffCompression.G4
                : TiffCompression.Lzw
        }));
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.Orientation, 1);
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.BitsPerSample, pixelFormat == ImagePixelFormat.BW1 ? 1 : 8);
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.SamplesPerPixel, pixelFormat switch
        {
            ImagePixelFormat.RGB24 => 3,
            ImagePixelFormat.ARGB32 => 4,
            _ => 1
        });
        LibTiff.TIFFSetFieldInt(tiff, TiffTag.Photometric, (int) (pixelFormat switch
        {
            ImagePixelFormat.RGB24 or ImagePixelFormat.ARGB32 => TiffPhotometric.Rgb,
            _ => TiffPhotometric.MinIsBlack
        }));
        if (pixelFormat == ImagePixelFormat.ARGB32)
        {
            LibTiff.TIFFSetFieldShortArray(tiff, TiffTag.ExtraSamples, 1, new short[] { 2 });
        }
        if (image.HorizontalResolution != 0 && image.VerticalResolution != 0)
        {
            LibTiff.TIFFSetFieldInt(tiff, TiffTag.ResolutionUnit, 2);
            // TODO: Why do we need to write as a double? It's supposed to be a float.
            LibTiff.TIFFSetFieldDouble(tiff, TiffTag.XResolution, image.HorizontalResolution);
            LibTiff.TIFFSetFieldDouble(tiff, TiffTag.YResolution, image.VerticalResolution);
        }
    }

    public void LoadTiff(Action<IMemoryImage> produceImage, Stream stream, ProgressHandler progress)
    {
        var client = new LibTiffStreamClient(stream);
        var tiff = client.TIFFClientOpen("r");
        EnumerateTiffFrames(produceImage, tiff, progress, client);
    }

    private void EnumerateTiffFrames(Action<IMemoryImage> produceImage, IntPtr tiff,
        ProgressHandler progress, LibTiffStreamClient client)
    {
        // We keep a reference to the client to avoid garbage collection
        var handle = GCHandle.Alloc(client);
        try
        {
            var count = LibTiff.TIFFNumberOfDirectories(tiff);
            progress.Report(0, count);
            int i = 0;
            do
            {
                if (progress.IsCancellationRequested) break;
                LibTiff.TIFFGetFieldInt(tiff, TiffTag.ImageWidth, out int w);
                LibTiff.TIFFGetFieldInt(tiff, TiffTag.ImageHeight, out int h);
                // TODO: Check return values
                LibTiff.TIFFGetFieldFloat(tiff, TiffTag.XResolution, out float xres);
                LibTiff.TIFFGetFieldFloat(tiff, TiffTag.YResolution, out float yres);
                var img = _imageContext.Create(w, h, ImagePixelFormat.ARGB32);
                img.SetResolution(xres, yres);
                img.OriginalFileFormat = ImageFileFormat.Tiff;
                using var imageLock = img.Lock(LockMode.WriteOnly, out var data);
                ReadTiffFrame(data.safePtr, tiff, w, h);
                imageLock.Dispose();
                // LibTiff always produces pre-multiplied alpha, which we don't want
                new UnmultiplyAlphaOp().Perform(img);
                progress.Report(++i, count);
                produceImage(img);
            } while (LibTiff.TIFFReadDirectory(tiff) == 1);
        }
        finally
        {
            LibTiff.TIFFClose(tiff);
            handle.Free();
        }
    }

    private static void ReadTiffFrame(IntPtr buffer, IntPtr tiff, int w, int h)
    {
        int r = LibTiff.TIFFReadRGBAImageOriented(tiff, w, h, buffer, 1, 0);
        // TODO: Check return value
    }
}
