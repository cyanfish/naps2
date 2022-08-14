using System.Collections.Immutable;

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
        return ImageFileFormat.Unspecified;
    }

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
            renderedPdf = _pdfRenderer.Render(this, fileStorage.FullPath, PdfRenderSize.FromDpi(300)).Single();
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
            renderedPdf = _pdfRenderer.Render(this, stream.GetBuffer(), (int) stream.Length, PdfRenderSize.FromDpi(300))
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

    public IMemoryImage Render(ProcessedImage processedImage)
    {
        var bitmap = RenderFromStorage(processedImage.Storage);
        return PerformAllTransforms(bitmap, processedImage.TransformState.Transforms);
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
                // Rather than creating a bitmap from the file directly, instead we read it into memory first.
                // This ensures we don't accidentally keep a lock on the storage file, which would cause an error if we
                // try to delete it before the bitmap is disposed.
                // This is less efficient in the case where the bitmap is guaranteed to be disposed quickly, but for now
                // that seems like a reasonable tradeoff to avoid a whole class of hard-to-diagnose errors.
                var stream = new MemoryStream(File.ReadAllBytes(fileStorage.FullPath));
                return Load(stream);
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

    public string SaveSmallestFormat(string pathWithoutExtension, IMemoryImage image, BitDepth bitDepth,
        bool lossless, int quality, out ImageFileFormat imageFileFormat)
    {
        // TODO: Should we save directly to the file?
        var memoryStream = SaveSmallestFormatToMemoryStream(image, bitDepth, lossless, quality, out imageFileFormat);
        var ext = imageFileFormat == ImageFileFormat.Png ? ".png" : ".jpg";
        var path = pathWithoutExtension + ext;
        using var fileStream = new FileStream(path, FileMode.Create);
        memoryStream.CopyTo(fileStream);
        return path;
    }

    public MemoryStream SaveSmallestFormatToMemoryStream(IMemoryImage image, BitDepth bitDepth, bool lossless,
        int quality, out ImageFileFormat imageFileFormat)
    {
        var exportFormat = GetExportFormat(image, bitDepth, lossless);
        if (exportFormat.FileFormat == ImageFileFormat.Png)
        {
            imageFileFormat = ImageFileFormat.Png;
            if (exportFormat.PixelFormat == ImagePixelFormat.BW1 && image.PixelFormat != ImagePixelFormat.BW1)
            {
                using var bwImage = PerformTransform(image.Clone(), new BlackWhiteTransform());
                return bwImage.SaveToMemoryStream(ImageFileFormat.Png);
            }
            return image.SaveToMemoryStream(ImageFileFormat.Png);
        }
        if (exportFormat.FileFormat == ImageFileFormat.Jpeg)
        {
            imageFileFormat = ImageFileFormat.Jpeg;
            return image.SaveToMemoryStream(ImageFileFormat.Jpeg, quality);
        }
        // Save as PNG/JPEG depending on which is smaller
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

    public ImageExportFormat GetExportFormat(IMemoryImage image, BitDepth bitDepth, bool lossless)
    {
        // Store the image in as little space as possible
        if (image.PixelFormat == ImagePixelFormat.BW1)
        {
            // Already encoded as 1-bit
            return new ImageExportFormat(ImageFileFormat.Png, ImagePixelFormat.BW1);
        }
        if (bitDepth == BitDepth.BlackAndWhite)
        {
            // Convert to a 1-bit bitmap before saving to help compression
            // This is lossless and takes up minimal storage (best of both worlds), so highQuality is irrelevant
            // Note that if a black and white image comes from native WIA, bitDepth is unknown,
            // so the image will be png-encoded below instead of using a 1-bit bitmap
            return new ImageExportFormat(ImageFileFormat.Png, ImagePixelFormat.BW1);
        }
        // TODO: Also for ARGB32? Or is OriginalFileFormat enough if we populate that more consistently?
        if (lossless || image.OriginalFileFormat == ImageFileFormat.Png)
        {
            // Store as PNG
            // Lossless, but some images (color/grayscale) take up lots of storage
            return new ImageExportFormat(ImageFileFormat.Png, image.PixelFormat);
        }
        if (image.OriginalFileFormat == ImageFileFormat.Jpeg)
        {
            // Store as JPEG
            // Since the image was originally in JPEG format, PNG is unlikely to have size benefits
            return new ImageExportFormat(ImageFileFormat.Jpeg, ImagePixelFormat.RGB24);
        }
        // No inherent preference for Jpeg or Png, the caller can decide
        return new ImageExportFormat(ImageFileFormat.Unspecified, ImagePixelFormat.RGB24);
    }
}