using System.Collections.Immutable;
using System.Reflection;

namespace NAPS2.Images.Storage;

public abstract class ImageContext : IDisposable
{
    private readonly Dictionary<(Type, Type), (object, MethodInfo)> _transformers = new();

    protected ImageContext(Type imageType)
    {
        ImageType = imageType;
        // _pdfRenderer = new PdfiumPdfRenderer(this);
    }

    /// <summary>
    /// Enumerates all methods on transformerObj that have a TransformerAttribute and registers them
    /// for future use in Transform.Perform and Transform.PerformAll with the specified image type.
    /// </summary>
    /// <param name="transformerObj"></param>
    public void RegisterTransformers<TImage>(object transformerObj) where TImage : IMemoryImage
    {
        foreach (var method in transformerObj.GetType().GetMethods().Where(x => x.GetCustomAttributes(typeof(TransformerAttribute), true).Any()))
        {
            var methodParams = method.GetParameters();
            var storageType = methodParams[0].ParameterType;
            var transformType = methodParams[1].ParameterType;
            if (methodParams.Length == 2 &&
                typeof(IMemoryImage).IsAssignableFrom(method.ReturnType) &&
                storageType.IsAssignableFrom(typeof(TImage)) &&
                typeof(Transform).IsAssignableFrom(transformType))
            {
                _transformers[(typeof(TImage), transformType)] = (transformerObj, method);
            }
        }
    }

    // TODO: Describe ownership transfer
    // TODO: Consider moving this to IImage
    /// <summary>
    /// Performs the specified transformation on the specified image using a compatible transformer.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="transform"></param>
    /// <returns></returns>
    public IMemoryImage PerformTransform(IMemoryImage image, Transform transform)
    {
        try
        {
            var (transformer, perform) = _transformers[(image.GetType(), transform.GetType())];
            return (IMemoryImage)perform.Invoke(transformer, new object[] { image, transform });
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"No transformer exists for {image.GetType().Name} and {transform.GetType().Name}");
        }
    }

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
            simplifiedTransforms = simplifiedTransforms.Add(transform);
        }
        return simplifiedTransforms.Aggregate(image, PerformTransform);
    }

    public Type ImageType { get; }

    public void ConfigureBackingStorage<TStorage>() where TStorage : IImageStorage
    {
        BackingStorageType = typeof(TStorage);
    }

    public Type BackingStorageType { get; private set; } = typeof(IImageStorage);

    // private IPdfRenderer _pdfRenderer;
    //
    // public IPdfRenderer PdfRenderer
    // {
    //     get => _pdfRenderer;
    //     set => _pdfRenderer = value ?? throw new ArgumentNullException(nameof(value));
    // }

    // public ImageContext UseFileStorage(string folderPath)
    // {
    //     FileStorageManager = new FileStorageManager(folderPath);
    //     ImageMetadataFactory = new StubImageMetadataFactory();
    //     ConfigureBackingStorage<FileStorage>();
    //     return this;
    // }
    //
    // public ImageContext UseFileStorage(FileStorageManager manager)
    // {
    //     FileStorageManager = manager;
    //     ImageMetadataFactory = new StubImageMetadataFactory();
    //     ConfigureBackingStorage<FileStorage>();
    //     return this;
    // }
    //
    // public ImageContext UseRecovery(string recoveryFolderPath)
    // {
    //     var rsm = RecoveryStorageManager.CreateFolder(recoveryFolderPath);
    //     FileStorageManager = rsm;
    //     ImageMetadataFactory = rsm;
    //     ConfigureBackingStorage<FileStorage>();
    //     return this;
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

    public abstract IMemoryImage Render(ProcessedImage processedImage);

    /// <summary>
    /// Creates a new empty image.
    /// </summary>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="pixelFormat">The image's pixel format.</param>
    /// <returns></returns>
    public abstract IMemoryImage Create(int width, int height, ImagePixelFormat pixelFormat);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // _fileStorageManager.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public abstract string SaveSmallestFormat(IMemoryImage image, string pathWithoutExtension, BitDepth bitDepth, bool highQuality, int quality, out ImageFileFormat imageFileFormat);
}
