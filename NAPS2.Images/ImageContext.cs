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
            _ => ImageFileFormat.Unknown
        };
    }

    private static ImageFileFormat GetFileFormatFromFirstBytes(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return ImageFileFormat.Unknown;
        }
        var firstBytes = new byte[8];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(firstBytes, 0, 8);
        stream.Seek(0, SeekOrigin.Begin);

        return GetFileFormatFromFirstBytes(firstBytes);
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
            _ => ImageFileFormat.Unknown
        };
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
            renderedPdf = pdfRenderer.Render(this, fileStorage.FullPath, PdfRenderSize.Default).Single();
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
            renderedPdf = pdfRenderer.Render(this, stream.GetBuffer(), (int) stream.Length, PdfRenderSize.Default)
                .Single();
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

    public bool SupportsFormat(ImageFileFormat format) =>
        format is ImageFileFormat.Bmp or ImageFileFormat.Jpeg or ImageFileFormat.Png ||
        format == ImageFileFormat.Tiff && SupportsTiff || format == ImageFileFormat.Jpeg2000 && SupportsJpeg2000;

    protected virtual bool SupportsTiff => false;
    protected virtual bool SupportsJpeg2000 => false;

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
        CheckSupportsFormat(format);
        var image = LoadCore(stream, format);
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
            CheckSupportsFormat(format);
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
                LoadFramesCore(produceImage, stream, format, progress);
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

    public void CheckSupportsFormat(ImageFileFormat format)
    {
        if (!SupportsFormat(format))
        {
            throw new NotSupportedException($"Unsupported file format: {format}");
        }
    }

    public virtual ITiffWriter TiffWriter => throw new NotSupportedException();

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