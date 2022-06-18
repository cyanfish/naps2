using System.Collections.Immutable;
using System.Windows.Forms;
using NAPS2.Ocr;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan;

public class ScanningContext : IDisposable
{
    private readonly ProcessedImageOwner _processedImageOwner = new();
    
    // TODO: Make sure properties are initialized by callers (or something equivalent)
    public ScanningContext(ImageContext imageContext)
    {
        ImageContext = imageContext;
    }

    public ScanningContext(ImageContext imageContext, FileStorageManager fileStorageManager)
    {
        ImageContext = imageContext;
        FileStorageManager = fileStorageManager;
    }

    public ScanningContext(ImageContext imageContext, FileStorageManager fileStorageManager, IOcrEngine ocrEngine)
    {
        ImageContext = imageContext;
        FileStorageManager = fileStorageManager;
        OcrEngine = ocrEngine;
    }

    // TODO: Figure out initialization etc.
    public ImageContext ImageContext { get; }

    public FileStorageManager? FileStorageManager { get; set; }

    // TODO: Rethink how this works.
    public string TempFolderPath { get; set; }

    public IWorkerFactory WorkerFactory { get; set; }

    public OcrRequestQueue OcrRequestQueue { get; } = new();

    public IOcrEngine? OcrEngine { get; set; }
    
    public ProcessedImage CreateProcessedImage(IImageStorage storage)
    {
        return CreateProcessedImage(storage, Enumerable.Empty<Transform>());
    }
    
    public ProcessedImage CreateProcessedImage(IImageStorage storage, IEnumerable<Transform> transforms)
    {
        return CreateProcessedImage(storage, BitDepth.Color, false, -1, transforms);
    }

    public ProcessedImage CreateProcessedImage(IImageStorage storage, BitDepth bitDepth, bool lossless, int quality, IEnumerable<Transform> transforms)
    {
        var convertedStorage = ConvertStorageIfNeeded(storage, bitDepth, lossless, quality);
        var metadata = new ImageMetadata(bitDepth, lossless);
        var image = new ProcessedImage(
            convertedStorage,
            metadata,
            new PostProcessingData(),
            new TransformState(transforms.ToImmutableList()),
            _processedImageOwner);
        return image;
    }

    private IImageStorage ConvertStorageIfNeeded(IImageStorage storage, BitDepth bitDepth, bool lossless, int quality)
    {
        switch (storage)
        {
            case IMemoryImage image:
                if (FileStorageManager == null)
                {
                    return image.Clone();
                }
                return WriteImageToBackingFile(image, bitDepth, lossless, quality);
            case ImageFileStorage fileStorage:
                if (FileStorageManager != null)
                {
                    return fileStorage;
                }
                return ImageContext.Load(fileStorage.FullPath);
            case MemoryStreamImageStorage memoryStreamStorage:
                var loadedImage = ImageContext.Load(memoryStreamStorage.Stream);
                if (FileStorageManager == null)
                {
                    return loadedImage;
                }
                return WriteImageToBackingFile(loadedImage, bitDepth, lossless, quality);
            default:
                // The only case that should hit this is a test with a mock
                return storage;
        }
        // TODO: It probably makes sense to abstract this based on the type of backend (filestorage/not)
    }

    private IImageStorage WriteImageToBackingFile(IMemoryImage image, BitDepth bitDepth, bool lossless, int quality)
    {
        if (FileStorageManager == null)
        {
            throw new InvalidOperationException();
        }
        var path = FileStorageManager.NextFilePath();
        var fullPath = ImageContext.SaveSmallestFormat(image, path, bitDepth, lossless, quality, out _);
        return new ImageFileStorage(fullPath, false);
    }

    public void Dispose()
    {
        _processedImageOwner.Dispose();
        FileStorageManager?.Dispose();
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