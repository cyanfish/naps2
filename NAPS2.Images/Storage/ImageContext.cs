using System.Collections.Immutable;
using System.Threading;

namespace NAPS2.Images.Storage;

public abstract class ImageContext
{
    private readonly IPdfRenderer? _pdfRenderer;

    // TODO: Any better place to put this?
    public static ImageFileFormat GetFileFormatFromExtension(string path, bool allowUnspecified = false)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => ImageFileFormat.Png,
            ".bmp" => ImageFileFormat.Bmp,
            ".jpg" or ".jpeg" => ImageFileFormat.Jpeg,
            ".tif" or ".tiff" => ImageFileFormat.Tiff,
            _ => allowUnspecified
                ? ImageFileFormat.Unspecified
                : throw new ArgumentException($"Could not infer file format from extension: {path}")
        };
    }

    public static ImageFileFormat GetFileFormatFromFirstBytes(Stream stream)
    {
        if (!stream.CanSeek)
        {
            return ImageFileFormat.Unspecified;
        }
        var firstBytes = new byte[4];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(firstBytes, 0, 4);
        stream.Seek(0, SeekOrigin.Begin);
        if (firstBytes[0] == 0x89 && firstBytes[1] == 0x50 && firstBytes[2] == 0x4E && firstBytes[3] == 0x47)
        {
            return ImageFileFormat.Png;
        }
        if (firstBytes[0] == 0xFF && firstBytes[1] == 0xD8)
        {
            return ImageFileFormat.Jpeg;
        }
        if (firstBytes[0] == 0x42 && firstBytes[1] == 0x4D)
        {
            return ImageFileFormat.Bmp;
        }
        if (firstBytes[0] is 0x4D or 0x49 && firstBytes[1] is 0x4D or 0x49 && firstBytes[2] == 0x2A &&
            firstBytes[3] == 0x00)
        {
            return ImageFileFormat.Tiff;
        }
        return ImageFileFormat.Unspecified;
    }

    protected ImageContext(Type imageType, IPdfRenderer? pdfRenderer = null)
    {
        ImageType = imageType;
        _pdfRenderer = pdfRenderer;
    }

    protected bool LoadFromFileKeepsLock { get; init; }

    // TODO: Add NotNullWhen attribute?
    private bool MaybeRenderPdf(ImageFileStorage fileStorage, out IMemoryImage? renderedPdf)
    {
        if (Path.GetExtension(fileStorage.FullPath).ToLowerInvariant() == ".pdf")
        {
            if (_pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the ImageContext wasn't created with an IPdfRenderer.");
            }
            renderedPdf = _pdfRenderer.Render(this, fileStorage.FullPath, PdfRenderSize.Default).Single();
            return true;
        }
        renderedPdf = null;
        return false;
    }

    private bool MaybeRenderPdf(ImageMemoryStorage memoryStorage, out IMemoryImage? renderedPdf)
    {
        if (memoryStorage.TypeHint == ".pdf")
        {
            if (_pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the ImageContext wasn't created with an IPdfRenderer.");
            }
            var stream = memoryStorage.Stream;
            renderedPdf = _pdfRenderer.Render(this, stream.GetBuffer(), (int) stream.Length, PdfRenderSize.Default)
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

    /// <summary>
    /// Loads an image from the given file path.
    /// </summary>
    /// <param name="path">The image path.</param>
    /// <returns></returns>
    public abstract IMemoryImage Load(string path);

    // TODO: The original method had an extension/fileformat. Is that relevant?
    // Old doc: A file extension hinting at the image format. When possible, the contents of the stream should be used to definitively determine the image format.
    /// <summary>
    /// Decodes an image from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <returns></returns>
    public abstract IMemoryImage Load(Stream stream);

    // TODO: Document
    public IMemoryImage Load(byte[] bytes)
    {
        return Load(new MemoryStream(bytes));
    }

    // TODO: The original doc said that only the currently enumerated image is guaranteed to be valid. Is this still true?
    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <param name="count">The number of returned images.</param>
    /// <returns></returns>
    public abstract IEnumerable<IMemoryImage> LoadFrames(Stream stream, out int count);

    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given file path.
    /// </summary>
    /// <param name="path">The image path.</param>
    /// <param name="count">The number of returned images.</param>
    /// <returns></returns>
    public abstract IEnumerable<IMemoryImage> LoadFrames(string path, out int count);

    // TODO: Instead of having these methods directly here, instead have a TiffWriter property
    public abstract bool SaveTiff(IList<IMemoryImage> images, string path,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default);

    public abstract bool SaveTiff(IList<IMemoryImage> images, Stream stream,
        TiffCompressionType compression = TiffCompressionType.Auto, Action<int, int>? progressCallback = null,
        CancellationToken cancelToken = default);

    public IMemoryImage Render(IRenderableImage image)
    {
        var bitmap = RenderFromStorage(image.Storage);
        return PerformAllTransforms(bitmap, image.TransformState.Transforms);
    }

    public IMemoryImage RenderFromStorage(IImageStorage storage)
    {
        switch (storage)
        {
            case ImageFileStorage fileStorage:
                if (MaybeRenderPdf(fileStorage, out var renderedPdf))
                {
                    return renderedPdf!;
                }
                if (LoadFromFileKeepsLock)
                {
                    // Rather than creating a bitmap from the file directly, instead we read it into memory first.
                    // This ensures we don't accidentally keep a lock on the storage file, which would cause an error if we
                    // try to delete it before the bitmap is disposed.
                    // This is less efficient in the case where the bitmap is guaranteed to be disposed quickly, but for now
                    // that seems like a reasonable tradeoff to avoid a whole class of hard-to-diagnose errors.
                    var stream = new MemoryStream(File.ReadAllBytes(fileStorage.FullPath));
                    return Load(stream);
                }
                return Load(fileStorage.FullPath);
            case ImageMemoryStorage memoryStorage:
                if (MaybeRenderPdf(memoryStorage, out var renderedMemoryPdf))
                {
                    return renderedMemoryPdf!;
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