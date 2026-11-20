using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using NAPS2.Util;

namespace NAPS2.Images;

public abstract class ImageContext
{
    public static ImageFileFormat GetFileFormatFromExtension(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => ImageFileFormat.Png,
            ".bmp" => ImageFileFormat.Bmp,
            ".jpg" or ".jpeg" => ImageFileFormat.Jpeg,
            ".tif" or ".tiff" => ImageFileFormat.Tiff,
            ".jp2" or ".jpx" => ImageFileFormat.Jpeg2000,
            ".webp" => ImageFileFormat.WebP,
            ".jxl" => ImageFileFormat.JpegXl,
            ".heic" or ".heif" => ImageFileFormat.Heic,
            ".avif" => ImageFileFormat.Avif,
            _ => ImageFileFormat.Unknown
        };
    }

    // Number of bytes from the start of a file needed to detect all formats.
    private const int FIRST_BYTES_LENGTH = 32;

    private static ImageFileFormat GetFileFormatFromFirstBytes(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return ImageFileFormat.Unknown;
        }
        return GetFileFormatFromFirstBytes(ReadFirstBytes(stream));
    }

    internal static byte[] ReadFirstBytes(Stream stream)
    {
        var buffer = new byte[FIRST_BYTES_LENGTH];
        stream.Seek(0, SeekOrigin.Begin);
        int length = 0;
        int read;
        while (length < buffer.Length && (read = stream.Read(buffer, length, buffer.Length - length)) > 0)
        {
            length += read;
        }
        stream.Seek(0, SeekOrigin.Begin);
        return length == buffer.Length ? buffer : buffer.Take(length).ToArray();
    }

    public static ImageFileFormat GetFileFormatFromFirstBytes(byte[] firstBytes)
    {
        return firstBytes switch
        {
            [0x89, 0x50, 0x4E, 0x47, ..] => ImageFileFormat.Png,
            [0xFF, 0xD8, ..] => ImageFileFormat.Jpeg,
            [0x42, 0x4D, ..] => ImageFileFormat.Bmp,
            [0x49, 0x49, 0x2A, 0x00, ..] => ImageFileFormat.Tiff,
            [0x4D, 0x4D, 0x00, 0x2A, ..] => ImageFileFormat.Tiff,
            [_, _, _, _, 0x6A, 0x50, 0x20, 0x20, ..] => ImageFileFormat.Jpeg2000,
            // "RIFF" <size> "WEBP"
            [0x52, 0x49, 0x46, 0x46, _, _, _, _, 0x57, 0x45, 0x42, 0x50, ..] => ImageFileFormat.WebP,
            // JPEG XL codestream, or the container's signature box
            [0xFF, 0x0A, ..] => ImageFileFormat.JpegXl,
            [0x00, 0x00, 0x00, 0x0C, 0x4A, 0x58, 0x4C, 0x20, 0x0D, 0x0A, 0x87, 0x0A, ..] => ImageFileFormat.JpegXl,
            // ISO base media file ("ftyp" box), which HEIC and AVIF both use
            [_, _, _, _, 0x66, 0x74, 0x79, 0x70, ..] => GetFormatFromFileTypeBox(firstBytes),
            _ => ImageFileFormat.Unknown
        };
    }

    private static ImageFileFormat GetFormatFromFileTypeBox(byte[] firstBytes)
    {
        // The box has a 4-byte size, "ftyp", the major brand, a 4-byte version, then any compatible brands
        int boxSize = (firstBytes[0] << 24) | (firstBytes[1] << 16) | (firstBytes[2] << 8) | firstBytes[3];
        int end = Math.Min(boxSize, firstBytes.Length);
        var brands = new List<string>();
        for (int i = 8; i + 4 <= end; i += 4)
        {
            if (i == 12) continue;
            brands.Add(System.Text.Encoding.ASCII.GetString(firstBytes, i, 4));
        }
        if (brands.Any(brand => brand is "avif" or "avis"))
        {
            return ImageFileFormat.Avif;
        }
        if (brands.Any(brand => brand is "heic" or "heix" or "heim" or "heis" or "hevc" or "hevx" or "mif1" or "msf1"))
        {
            return ImageFileFormat.Heic;
        }
        return ImageFileFormat.Unknown;
    }

    protected ImageContext(Type imageType)
    {
        ImageType = imageType;
    }

    private bool MaybeRenderPdf(ImageFileStorage fileStorage, IPdfRenderer? pdfRenderer,
        [NotNullWhen(true)] out IMemoryImage? renderedPdf)
    {
        if (Path.GetExtension(fileStorage.FullPath).ToLowerInvariant() == ".pdf")
        {
            if (pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the IRenderableImage didn't implement IPdfRendererProvider.");
            }
            renderedPdf = pdfRenderer.RenderPage(this, fileStorage.FullPath, PdfRenderSize.Default);
            return true;
        }
        renderedPdf = null;
        return false;
    }

    private bool MaybeRenderPdf(ImageMemoryStorage memoryStorage, IPdfRenderer? pdfRenderer,
        [NotNullWhen(true)] out IMemoryImage? renderedPdf)
    {
        if (memoryStorage.TypeHint == ".pdf")
        {
            if (pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the IRenderableImage didn't implement IPdfRendererProvider.");
            }
            var stream = memoryStorage.Stream;
            renderedPdf = pdfRenderer.RenderPage(this, stream.GetBuffer(), (int) stream.Length, PdfRenderSize.Default);
            return true;
        }
        renderedPdf = null;
        return false;
    }

    // TODO: Describe ownership transfer
    /// <summary>
    /// Performs the specified transformation on the specified image using a compatible transformer.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public abstract IMemoryImage PerformTransform(IMemoryImage image, Transform transform);

    /// <summary>
    /// Performs the specified transformations on the specified image using a compatible transformer.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="transforms"></param>
    /// <returns></returns>
    public IMemoryImage PerformAllTransforms(IMemoryImage image, IEnumerable<Transform> transforms)
    {
        var simplifiedTransforms = ImmutableList<Transform>.Empty;
        foreach (var transform in transforms)
        {
            // TODO: Simplify
            simplifiedTransforms = simplifiedTransforms.Add(transform);
        }
        return simplifiedTransforms.Aggregate(image, PerformTransform);
    }

    public Type ImageType { get; }

    public bool SupportsFormat(ImageFileFormat format) => SupportsDecode(format) && SupportsEncode(format);

    public bool SupportsDecode(ImageFileFormat format) =>
        BundledEncoders.ContainsKey(format) || format switch
        {
            ImageFileFormat.Jpeg or ImageFileFormat.Png => true,
            ImageFileFormat.Bmp => SupportsBmpDecode,
            ImageFileFormat.Tiff => SupportsTiffDecode,
            ImageFileFormat.Jpeg2000 => SupportsJpeg2000Decode,
            ImageFileFormat.Heic => SupportsHeicDecode,
            ImageFileFormat.Avif => SupportsAvifDecode,
            _ => false
        };

    public bool SupportsEncode(ImageFileFormat format) =>
        BundledEncoders.ContainsKey(format) || format switch
        {
            ImageFileFormat.Jpeg or ImageFileFormat.Png => true,
            ImageFileFormat.Bmp => SupportsBmpEncode,
            ImageFileFormat.Tiff => SupportsTiffEncode,
            ImageFileFormat.Jpeg2000 => SupportsJpeg2000Encode,
            ImageFileFormat.Heic => SupportsHeicEncode,
            ImageFileFormat.Avif => SupportsAvifEncode,
            _ => false
        };

    protected virtual bool SupportsBmpDecode => true;
    protected virtual bool SupportsBmpEncode => true;
    protected virtual bool SupportsTiffDecode => false;
    protected virtual bool SupportsTiffEncode => false;
    protected virtual bool SupportsJpeg2000Decode => false;
    protected virtual bool SupportsJpeg2000Encode => false;
    protected virtual bool SupportsHeicDecode => false;
    protected virtual bool SupportsHeicEncode => false;
    protected virtual bool SupportsAvifDecode => false;
    protected virtual bool SupportsAvifEncode => false;

    /// <summary>
    /// Loads an image from the given file path.
    /// </summary>
    /// <param name="path">The image path.</param>
    /// <returns></returns>
    public IMemoryImage Load(string path)
    {
        using var stream = File.OpenRead(path);
        return Load(stream);
    }

    /// <summary>
    /// Decodes an image from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <returns></returns>
    public IMemoryImage Load(Stream stream)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        CheckSupportsDecode(format);
        var image = TryBundledDecode(stream, format, out var decoded) ? decoded : LoadCore(stream, format);
        if (image.OriginalFileFormat == ImageFileFormat.Unknown)
        {
            image.OriginalFileFormat = format;
        }
        return image;
    }

    protected abstract IMemoryImage LoadCore(Stream stream, ImageFileFormat format);

    public IMemoryImage Load(byte[] bytes)
    {
        return Load(new MemoryStream(bytes));
    }

    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <param name="progress">The progress callback and/or cancellation token.</param>
    /// <returns></returns>
    public IAsyncEnumerable<IMemoryImage> LoadFrames(Stream stream, ProgressHandler progress = default)
    {
        var format = GetFileFormatFromFirstBytes(stream);
        var source = DoLoadFrames(stream, format, progress, false);
        return WrapSource(source, format);
    }

    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given file path.
    /// </summary>
    /// <param name="path">The image path.</param>
    /// <param name="progress">The progress callback and/or cancellation token.</param>
    /// <returns></returns>
    public IAsyncEnumerable<IMemoryImage> LoadFrames(string path, ProgressHandler progress = default)
    {
        var stream = File.OpenRead(path);
        var format = GetFileFormatFromFirstBytes(stream);
        var source = DoLoadFrames(stream, format, progress, true);
        return WrapSource(source, format);
    }

    private IAsyncEnumerable<IMemoryImage> DoLoadFrames(Stream stream, ImageFileFormat format, ProgressHandler progress,
        bool disposeStream)
    {
        try
        {
            CheckSupportsDecode(format);
        }
        catch (Exception)
        {
            if (disposeStream) stream.Dispose();
            throw;
        }
        return AsyncProducers.RunProducer<IMemoryImage>(produceImage =>
        {
            try
            {
                if (TryBundledDecode(stream, format, out var image))
                {
                    progress.Report(0, 1);
                    produceImage(image);
                    progress.Report(1, 1);
                }
                else
                {
                    LoadFramesCore(produceImage, stream, format, progress);
                }
            }
            finally
            {
                if (disposeStream)
                {
                    stream.Dispose();
                }
            }
        });
    }

    protected abstract void LoadFramesCore(Action<IMemoryImage> produceImage, Stream stream,
        ImageFileFormat format, ProgressHandler progress);

    private async IAsyncEnumerable<IMemoryImage> WrapSource(IAsyncEnumerable<IMemoryImage> source,
        ImageFileFormat format)
    {
        await foreach (var image in source)
        {
            if (image.OriginalFileFormat == ImageFileFormat.Unknown)
            {
                image.OriginalFileFormat = format;
            }
            yield return image;
        }
    }

    public void CheckSupportsDecode(ImageFileFormat format)
    {
        if (!SupportsDecode(format))
        {
            throw new NotSupportedException($"Unsupported file format for loading: {format}");
        }
    }

    public void CheckSupportsEncode(ImageFileFormat format)
    {
        if (!SupportsEncode(format))
        {
            throw new NotSupportedException($"Unsupported file format for saving: {format}");
        }
    }

    public virtual ITiffWriter TiffWriter => throw new NotSupportedException();

    /// <summary>
    /// Encoders to use instead of the platform encoder for specific file formats. This is process-wide, as images
    /// don't necessarily share the ImageContext instance they were created from.
    /// </summary>
    internal static ConcurrentDictionary<ImageFileFormat, IBundledEncoder> BundledEncoders { get; } = new();

    /// <summary>
    /// Loads the image with a bundled encoder if there's one for the given format. Returns false if the platform decoder
    /// should be used instead.
    /// </summary>
    private bool TryBundledDecode(Stream stream, ImageFileFormat format, [NotNullWhen(true)] out IMemoryImage? image)
    {
        image = null;
        return BundledEncoders.TryGetValue(format, out var encoder) && encoder.TryDecode(this, stream, out image);
    }

    /// <summary>
    /// Saves the image with a bundled encoder if there's one for the given format. Returns false if the platform encoder
    /// should be used instead.
    /// </summary>
    internal bool TryBundledEncode(IMemoryImage image, Stream stream, ImageFileFormat format,
        ImageSaveOptions? options)
    {
        return BundledEncoders.TryGetValue(format, out var encoder) &&
               encoder.TryEncode(image, stream, options ?? new ImageSaveOptions());
    }

    /// <summary>
    /// Saves the image with a bundled encoder if there's one for the given format. Returns false if the platform encoder
    /// should be used instead.
    /// </summary>
    internal bool TryBundledEncode(IMemoryImage image, string path, ImageFileFormat format,
        ImageSaveOptions? options)
    {
        if (!BundledEncoders.TryGetValue(format, out var encoder))
        {
            return false;
        }
        bool success;
        using (var stream = new FileStream(path, FileMode.Create))
        {
            success = encoder.TryEncode(image, stream, options ?? new ImageSaveOptions());
        }
        if (!success)
        {
            // Don't leave an empty file behind if the platform encoder fails too
            File.Delete(path);
        }
        return success;
    }

    public IMemoryImage Render(IRenderableImage image)
    {
        var bitmap = RenderWithoutTransforms(image);
        return PerformAllTransforms(bitmap, image.TransformState.Transforms);
    }

    public IMemoryImage RenderWithoutTransforms(IRenderableImage image)
    {
        return RenderFromStorage(image.Storage, (image as IPdfRendererProvider)?.PdfRenderer);
    }

    private IMemoryImage RenderFromStorage(IImageStorage storage, IPdfRenderer? pdfRenderer)
    {
        switch (storage)
        {
            case ImageFileStorage fileStorage:
                if (MaybeRenderPdf(fileStorage, pdfRenderer, out var renderedPdf))
                {
                    return renderedPdf;
                }
                return Load(fileStorage.FullPath);
            case ImageMemoryStorage memoryStorage:
                if (MaybeRenderPdf(memoryStorage, pdfRenderer, out var renderedMemoryPdf))
                {
                    return renderedMemoryPdf;
                }
                return Load(memoryStorage.Stream);
            case IMemoryImage image:
                return image.Clone();
        }
        throw new ArgumentException("Unsupported image storage: " + storage);
    }

    /// <summary>
    /// Creates a new empty image.
    /// </summary>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="pixelFormat">The image's pixel format.</param>
    /// <returns></returns>
    public abstract IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat);
}