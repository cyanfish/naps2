using System.Collections.Immutable;

namespace NAPS2.Images.Storage;

public abstract class ImageContext
{
    private readonly IPdfRenderer? _pdfRenderer;

    protected ImageContext(Type imageType, IPdfRenderer? pdfRenderer = null)
    {
        ImageType = imageType;
        _pdfRenderer = pdfRenderer;
    }

    // TODO: Add NotNullWhen attribute?
    protected bool MaybeRenderPdf(ImageFileStorage fileStorage, out IMemoryImage? renderedPdf)
    {
        if (Path.GetExtension(fileStorage.FullPath).ToLowerInvariant() == ".pdf")
        {
            if (_pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the ImageContext wasn't created with an IPdfRenderer.");
            }
            renderedPdf = _pdfRenderer.Render(this, fileStorage.FullPath, 300).Single();
            return true;
        }
        renderedPdf = null;
        return false;
    }

    protected bool MaybeRenderPdf(ImageMemoryStorage memoryStorage, out IMemoryImage? renderedPdf)
    {
        if (memoryStorage.TypeHint == ".pdf")
        {
            if (_pdfRenderer == null)
            {
                throw new InvalidOperationException(
                    "Unable to render pdf page as the ImageContext wasn't created with an IPdfRenderer.");
            }
            var stream = memoryStorage.Stream;
            renderedPdf = _pdfRenderer.Render(this, stream.GetBuffer(), (int) stream.Length, 300).Single();
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

    // private IPdfRenderer _pdfRenderer;
    //
    // public IPdfRenderer PdfRenderer
    // {
    //     get => _pdfRenderer;
    //     set => _pdfRenderer = value ?? throw new ArgumentNullException(nameof(value));
    // }

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

    public IMemoryImage Render(ProcessedImage processedImage)
    {
        var bitmap = RenderFromStorage(processedImage.Storage);
        return PerformAllTransforms(bitmap, processedImage.TransformState.Transforms);
    }

    public abstract IMemoryImage RenderFromStorage(IImageStorage storage);

    /// <summary>
    /// Creates a new empty image.
    /// </summary>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="pixelFormat">The image's pixel format.</param>
    /// <returns></returns>
    public abstract IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat);

    public string SaveSmallestFormat(string pathWithoutExtension, IMemoryImage image, BitDepth bitDepth,
        bool lossless, int quality, out ImageFileFormat imageFileFormat, bool encodeOnce = false)
    {
        var memoryStream = SaveSmallestFormatToMemoryStream(image, bitDepth, lossless, quality, out imageFileFormat);
        var ext = imageFileFormat == ImageFileFormat.Png ? ".png" : ".jpg";
        var path = pathWithoutExtension + ext;
        using var fileStream = new FileStream(path, FileMode.Create);
        memoryStream.CopyTo(fileStream);
        return path;
    }

    public MemoryStream SaveSmallestFormatToMemoryStream(IMemoryImage image, BitDepth bitDepth, bool lossless,
        int quality, out ImageFileFormat imageFileFormat, bool encodeOnce = false)
    {
        // Store the image in as little space as possible
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            // Already encoded as 1-bit
            imageFileFormat = ImageFileFormat.Png;
            return image.SaveToMemoryStream(ImageFileFormat.Png);
        }
        if (bitDepth == BitDepth.BlackAndWhite)
        {
            // Convert to a 1-bit bitmap before saving to help compression
            // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
            using var bwImage = PerformTransform(image.Clone(), new BlackWhiteTransform());
            imageFileFormat = ImageFileFormat.Png;
            return bwImage.SaveToMemoryStream(ImageFileFormat.Png);
            // Note that if a black and white image comes from native WIA, bitDepth is unknown,
            // so the image will be png-encoded below instead of using a 1-bit bitmap
        }
        if (lossless)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            imageFileFormat = ImageFileFormat.Png;
            return image.SaveToMemoryStream(ImageFileFormat.Png);
        }
        if (image.OriginalFileFormat == ImageFileFormat.Jpeg)
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            imageFileFormat = ImageFileFormat.Jpeg;
            return image.SaveToMemoryStream(ImageFileFormat.Jpeg);
        }
        if (encodeOnce)
        {
            // If the caller doesn't want to do an extra encode for the chance of a smaller image, just go with jpeg
            imageFileFormat = ImageFileFormat.Jpeg;
            return image.SaveToMemoryStream(ImageFileFormat.Jpeg);
        }
        // Store as PNG/JPEG depending on which is smaller
        var pngEncoded = image.SaveToMemoryStream(ImageFileFormat.Png);
        var jpegEncoded = image.SaveToMemoryStream(ImageFileFormat.Jpeg, quality);
        if (pngEncoded.Length <= jpegEncoded.Length)
        {
            // Probably a black and white image (e.g. from native WIA, where bitDepth is unknown), which PNG compresses well vs. JPEG
            imageFileFormat = ImageFileFormat.Png;
            return pngEncoded;
        }
        // Probably a color or grayscale image, which JPEG compresses well vs. PNG
        imageFileFormat = ImageFileFormat.Jpeg;
        return jpegEncoded;
    }
}