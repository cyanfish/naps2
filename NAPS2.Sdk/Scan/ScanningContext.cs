using System.Collections.Immutable;
using NAPS2.Ocr;
using NAPS2.Remoting.Worker;

namespace NAPS2.Scan;

public class ScanningContext : IDisposable
{
    // TODO: Make sure properties are initialized by callers (or something equivalent)
    public ScanningContext(ImageContext imageContext)
    {
        ImageContext = imageContext;
    }

    // TODO: Figure out initialization etc.
    public ImageContext ImageContext { get; }

    public FileStorageManager? FileStorageManager { get; set; }

    // TODO: Rethink how this works.
    public string TempFolderPath { get; set; }

    public IWorkerFactory WorkerFactory { get; set; }

    public OcrRequestQueue OcrRequestQueue { get; set; }
    
    public RenderableImage CreateRenderableImage(IStorage storage, BitDepth bitDepth, bool lossless, int quality, IEnumerable<Transform> transforms)
    {
        var convertedStorage = ConvertStorageIfNeeded(storage, bitDepth, lossless, quality);
        var metadata = new ImageMetadata(bitDepth, lossless);
        return new RenderableImage(convertedStorage, metadata, new TransformState(transforms.ToImmutableList()));
    }

    private IStorage ConvertStorageIfNeeded(IStorage storage, BitDepth bitDepth, bool lossless, int quality)
    {
        switch (storage)
        {
            case IImage image:
                if (FileStorageManager == null)
                {
                    return image.Clone();
                }
                return WriteImageToBackingFile(image, bitDepth, lossless, quality);
            case FileStorage fileStorage:
                if (FileStorageManager != null)
                {
                    return fileStorage;
                }
                return ImageContext.Load(fileStorage.FullPath);
            case MemoryStreamStorage memoryStreamStorage:
                var loadedImage = ImageContext.Load(memoryStreamStorage.Stream);
                if (FileStorageManager == null)
                {
                    return loadedImage;
                }
                return WriteImageToBackingFile(loadedImage, bitDepth, lossless, quality);
        }
        // TODO: Any other cases or issues here?
        // TODO: Also it probably makes sense to abstract this based on the type of backend (filestorage/not)
        throw new ArgumentException();
    }

    private IStorage WriteImageToBackingFile(IImage image, BitDepth bitDepth, bool lossless, int quality)
    {
        if (FileStorageManager == null)
        {
            throw new InvalidOperationException();
        }
        var path = FileStorageManager.NextFilePath();
        var fullPath = ImageContext.SaveSmallestFormat(image, path, bitDepth, lossless, quality, out _);
        return new FileStorage(fullPath, false);
    }

    public void Dispose()
    {
        // TODO: Dispose images created via this scanning context
        ImageContext.Dispose();
        FileStorageManager?.Dispose();
    }
}
