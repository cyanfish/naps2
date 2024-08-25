using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NAPS2.Ocr;
using NAPS2.Remoting.Worker;
using NAPS2.Scan.Internal;

namespace NAPS2.Scan;

/// <summary>
/// A ScanningContext object is needed for most NAPS2 operations. Set it up with the corresponding ImageContext type
/// for image type you expect (e.g. GdiImageContext for System.Drawing.Bitmap, if you're using Windows Forms). You can
/// also set various other properties that affect scanning and image processing.
/// <para/>
/// When the ScanningContext is disposed, all ProcessedImage objects that were generating from scanning or importing
/// with that ScanningContext object will be automatically disposed.
/// </summary>
public class ScanningContext : IDisposable
{
    private readonly ProcessedImageOwner _processedImageOwner = new();

    /// <summary>
    /// Initializes a new instance of the ScanningContext class with the specified ImageContext.
    /// </summary>
    /// <param name="imageContext">The corresponding ImageContext type for the image type you expect (e.g.
    /// GdiImageContext for System.Drawing.Bitmap, if you're using Windows Forms).</param>
    public ScanningContext(ImageContext imageContext)
    {
        ImageContext = imageContext;
    }

    /// <summary>
    /// Gets the context's ImageContext. This corresponds to the image type used (e.g. GdiImageContext for
    /// System.Drawing.Bitmap, if you're using Windows Forms).
    /// </summary>
    public ImageContext ImageContext { get; }

    /// <summary>
    /// Gets or sets the context's FileStorageManager. If non-null, ProcessedImage objects from scanning or importing
    /// with this ScanningContext will store the actual image data on disk instead of in memory.
    /// </summary>
    public FileStorageManager? FileStorageManager { get; set; }

    /// <summary>
    /// Gets or sets the context's WorkerFactory. This is required for some operations that need to happen in a worker
    /// process (e.g. to scan with 32-bit TWAIN from a 64-bit process).
    /// </summary>
    internal IWorkerFactory? WorkerFactory { get; set; }

    /// <summary>
    /// Gets or sets the context's OcrEngine. This is used to perform the OCR (optical character recognition) operation
    /// if OCR is requested for PDF export.
    /// </summary>
    public IOcrEngine? OcrEngine { get; set; }

    /// <summary>
    /// Gets or sets the path to a temp folder where transient files can be stored. Defaults to Path.GetTempPath().
    /// </summary>
    public string TempFolderPath { get; set; } = Path.GetTempPath();

    /// <summary>
    /// Gets or sets the logger used for detailed diagnostics.
    /// </summary>
    public ILogger Logger { get; set; } = NullLogger.Instance;

    internal string? RecoveryPath { get; set; }

    internal OcrRequestQueue OcrRequestQueue { get; } = new();

    public void Dispose()
    {
        _processedImageOwner.Dispose();
        FileStorageManager?.Dispose();
    }

    internal WorkerContext? CreateWorker(WorkerType workerType)
    {
        return WorkerFactory?.Create(this, workerType);
    }

    internal ProcessedImage CreateProcessedImage(IImageStorage storage)
    {
        return CreateProcessedImage(storage, Enumerable.Empty<Transform>());
    }

    internal ProcessedImage CreateProcessedImage(IImageStorage storage, IEnumerable<Transform> transforms)
    {
        return CreateProcessedImage(storage, false, -1, null, transforms);
    }

    internal ProcessedImage CreateProcessedImage(IImageStorage storage, bool lossless, int quality,
        PageSize? pageSize)
    {
        return CreateProcessedImage(storage, lossless, quality, pageSize, Enumerable.Empty<Transform>());
    }

    internal ProcessedImage CreateProcessedImage(IImageStorage storage, bool lossless, int quality,
        PageSize? pageSize, IEnumerable<Transform> transforms)
    {
        var convertedStorage = ConvertStorageIfNeeded(storage, lossless, quality);
        var metadata = new ImageMetadata(lossless, pageSize);
        var image = new ProcessedImage(
            ImageContext,
            convertedStorage,
            metadata,
            new PostProcessingData(),
            new TransformState(transforms.ToImmutableList()),
            _processedImageOwner);
        return image;
    }

    private IImageStorage ConvertStorageIfNeeded(IImageStorage storage, bool lossless, int quality)
    {
        if (FileStorageManager != null)
        {
            return ConvertToFileStorage(storage, lossless, quality);
        }
        return ConvertToMemoryStorage(storage);
    }

    private IImageStorage ConvertToMemoryStorage(IImageStorage storage)
    {
        switch (storage)
        {
            case IMemoryImage image:
                return image.Clone();
            case ImageFileStorage fileStorage:
                return ImageContext.Load(fileStorage.FullPath);
            case ImageMemoryStorage memoryStorage:
                if (memoryStorage.TypeHint == ".pdf")
                {
                    return memoryStorage;
                }
                return ImageContext.Load(memoryStorage.Stream);
            default:
                // The only case that should hit this is a test with a mock
                return storage;
        }
    }

    private IImageStorage ConvertToFileStorage(IImageStorage storage, bool lossless, int quality)
    {
        switch (storage)
        {
            case IMemoryImage image:
                return WriteImageToBackingFile(image, lossless, quality);
            case ImageFileStorage fileStorage:
                return fileStorage;
            case ImageMemoryStorage memoryStorage:
                if (memoryStorage.TypeHint == ".pdf")
                {
                    return WriteDataToBackingFile(memoryStorage.Stream, ".pdf");
                }
                // TODO: Can we just write this to a file directly? Is there any case where SaveSmallestFormat is really needed?
                var loadedImage = ImageContext.Load(memoryStorage.Stream);
                return WriteImageToBackingFile(loadedImage, lossless, quality);
            default:
                // The only case that should hit this is a test with a mock
                return storage;
        }
    }

    private ImageFileStorage WriteDataToBackingFile(MemoryStream stream, string ext)
    {
        if (FileStorageManager == null)
        {
            throw new InvalidOperationException();
        }
        var path = FileStorageManager.NextFilePath() + ext;
        using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        stream.WriteTo(fileStream);
        return new ImageFileStorage(path, false);
    }

    private IImageStorage WriteImageToBackingFile(IMemoryImage image, bool lossless, int quality)
    {
        if (FileStorageManager == null)
        {
            throw new InvalidOperationException();
        }
        var path = FileStorageManager.NextFilePath();
        var fullPath = ImageExportHelper.SaveSmallestFormat(path, image, lossless, quality, out _);
        return new ImageFileStorage(fullPath, false);
    }

    internal string SaveToTempFile(IMemoryImage image)
    {
        var path = Path.Combine(TempFolderPath, Path.GetRandomFileName());
        return ImageExportHelper.SaveSmallestFormat(path, image, false, -1, out _);
    }

    internal string SaveToTempFile(ProcessedImage image)
    {
        using var rendered = image.Render();
        return SaveToTempFile(rendered);
    }

    private class ProcessedImageOwner : IProcessedImageOwner, IDisposable
    {
        private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();

        public void Register(IDisposable internalDisposable)
        {
            lock (this)
            {
                _disposables.Add(internalDisposable);
            }
        }

        public void Unregister(IDisposable internalDisposable)
        {
            lock (this)
            {
                _disposables.Remove(internalDisposable);
            }
        }

        public void Dispose()
        {
            IEnumerable<IDisposable> list;
            lock (this)
            {
                list = _disposables.ToList();
            }
            foreach (var disposable in list)
            {
                disposable.Dispose();
            }
        }
    }
}