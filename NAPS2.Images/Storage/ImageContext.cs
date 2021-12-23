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
    public void RegisterTransformers<TImage>(object transformerObj) where TImage : IImage
    {
        foreach (var method in transformerObj.GetType().GetMethods().Where(x => x.GetCustomAttributes(typeof(TransformerAttribute), true).Any()))
        {
            var methodParams = method.GetParameters();
            var storageType = methodParams[0].ParameterType;
            var transformType = methodParams[1].ParameterType;
            if (methodParams.Length == 2 &&
                typeof(IImage).IsAssignableFrom(method.ReturnType) &&
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
    public IImage PerformTransform(IImage image, Transform transform)
    {
        try
        {
            var (transformer, perform) = _transformers[(image.GetType(), transform.GetType())];
            return (IImage)perform.Invoke(transformer, new object[] { image, transform });
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
    // TODO: Also perform simplification here
    public IImage PerformAllTransforms(IImage image, IEnumerable<Transform> transforms) => transforms.Aggregate(image, PerformTransform);

    public Type ImageType { get; }

    public void ConfigureBackingStorage<TStorage>() where TStorage : IStorage
    {
        BackingStorageType = typeof(TStorage);
    }

    public Type BackingStorageType { get; private set; } = typeof(IStorage);

    public IImageMetadataFactory ImageMetadataFactory { get; set; } = new StubImageMetadataFactory();

    // public ScannedImage CreateScannedImage(IStorage storage)
    // {
    //     return CreateScannedImage(storage, new StorageConvertParams());
    // }
    //
    // public ScannedImage CreateScannedImage(IStorage storage, StorageConvertParams convertParams)
    // {
    //     var backingStorage = ConvertToBacking(storage, convertParams);
    //     var metadata = ImageMetadataFactory.CreateMetadata(backingStorage);
    //     metadata.Commit();
    //     return new ScannedImage(backingStorage, metadata);
    // }
    //
    // public ScannedImage CreateScannedImage(IStorage storage, IImageMetadata metadata, StorageConvertParams convertParams)
    // {
    //     var backingStorage = ConvertToBacking(storage, convertParams);
    //     return new ScannedImage(backingStorage, metadata);
    // }
    //
    // public ScannedImage CreateScannedImage(IStorage storage, BitDepth bitDepth, bool highQuality, int quality)
    // {
    //     var convertParams = new StorageConvertParams { Lossless = highQuality, LossyQuality = quality, BitDepth = bitDepth };
    //     var backingStorage = ConvertToBacking(storage, convertParams);
    //     var metadata = ImageMetadataFactory.CreateMetadata(backingStorage);
    //     metadata.BitDepth = bitDepth;
    //     metadata.Lossless = highQuality;
    //     metadata.Commit();
    //     return new ScannedImage(backingStorage, metadata);
    // }

    // private FileStorageManager _fileStorageManager = new FileStorageManager();
    //
    // public FileStorageManager FileStorageManager
    // {
    //     get => _fileStorageManager;
    //     set => _fileStorageManager = value ?? throw new ArgumentNullException(nameof(value));
    // }

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
    public abstract IImage Load(string path);

    // TODO: The original method had an extension/fileformat. Is that relevant?
    // Old doc: A file extension hinting at the image format. When possible, the contents of the stream should be used to definitively determine the image format.
    /// <summary>
    /// Decodes an image from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <returns></returns>
    public abstract IImage Load(Stream stream);

    // TODO: The original doc said that only the currently enumerated image is guaranteed to be valid. Is this still true?
    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given stream.
    /// </summary>
    /// <param name="stream">The image data, in a common format (JPEG, PNG, etc).</param>
    /// <param name="count">The number of returned images.</param>
    /// <returns></returns>
    public abstract IEnumerable<IImage> LoadFrames(Stream stream, out int count);

    /// <summary>
    /// Loads an image that may have multiple frames (e.g. a TIFF file) from the given file path.
    /// </summary>
    /// <param name="path">The image path.</param>
    /// <param name="count">The number of returned images.</param>
    /// <returns></returns>
    public abstract IEnumerable<IImage> LoadFrames(string path, out int count);

    public abstract IImage Render(RenderableImage renderableImage);

    /// <summary>
    /// Creates a new empty image.
    /// </summary>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="pixelFormat">The image's pixel format.</param>
    /// <returns></returns>
    public abstract IImage Create(int width, int height, ImagePixelFormat pixelFormat);

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

    public abstract string SaveSmallestFormat(IImage image, string pathWithoutExtension, BitDepth bitDepth, bool highQuality, int quality, out ImageFileFormat imageFileFormat);
}
