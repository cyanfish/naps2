using System.Collections.Immutable;
using System.Threading;
using NAPS2.ImportExport.Images;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Recovery;

public class RecoverableFolder : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly ImportPostProcessor _importPostProcessor;
    private readonly DirectoryInfo _directory;
    private readonly FileStream _lockFile;
    private readonly RecoveryIndex _recoveryIndex;
    private bool _disposed;

    public RecoverableFolder(ScanningContext scanningContext, ImportPostProcessor importPostProcessor,
        DirectoryInfo directory)
    {
        _scanningContext = scanningContext;
        _importPostProcessor = importPostProcessor;
        _directory = directory;
        string lockFilePath = Path.Combine(directory.FullName, RecoveryStorageManager.LOCK_FILE_NAME);
        _lockFile = new FileStream(lockFilePath, FileMode.Open);
        try
        {
            var serializer = new XmlSerializer<RecoveryIndex>();
            _recoveryIndex = serializer.DeserializeFromFile(Path.Combine(directory.FullName, "index.xml"));
            ImageCount = _recoveryIndex.Images.Count;
            ScannedDateTime = directory.LastWriteTime;
            // TODO: Consider auto-delete in this case
            if (ImageCount == 0) throw new ArgumentException("No images to recover in this folder");
        }
        catch (Exception)
        {
            _lockFile.Dispose();
            throw;
        }
    }

    public int ImageCount { get; }

    public DateTime ScannedDateTime { get; }

    public void Dispose()
    {
        _lockFile.Dispose();
        _disposed = true;
    }

    public void TryDelete()
    {
        Dispose();
        try
        {
            _directory.Delete(true);
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error deleting recovery folder.", ex);
        }
    }

    public bool TryRecover(Action<ProcessedImage> imageCallback, RecoveryParams recoveryParams,
        ProgressHandler progressCallback, CancellationToken cancelToken)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RecoverableFolder));

        int currentProgress = 0;
        int totalProgress = ImageCount;
        progressCallback(currentProgress, totalProgress);

        foreach (RecoveryIndexImage indexImage in _recoveryIndex.Images)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return false;
            }

            string imagePath = Path.Combine(_directory.FullName, indexImage.FileName!);

            if (".pdf".Equals(Path.GetExtension(imagePath), StringComparison.InvariantCultureIgnoreCase))
            {
                string newPath = _scanningContext.FileStorageManager!.NextFilePath() + ".pdf";
                File.Copy(imagePath, newPath);
                var storage = new ImageFileStorage(newPath);
                var recoveredImage = CreateRecoveredImage(recoveryParams, storage, indexImage);
                imageCallback(recoveredImage);
            }
            else
            {
                // TODO: Why not do a file copy here too?
                using var storage = _scanningContext.ImageContext.Load(imagePath);
                var recoveredImage = CreateRecoveredImage(recoveryParams, storage, indexImage);
                imageCallback(recoveredImage);
            }

            currentProgress++;
            progressCallback(currentProgress, totalProgress);
        }
        // Now that we've recovered successfully, we can safely delete the old folder
        TryDelete();
        return true;
    }

    private ProcessedImage CreateRecoveredImage(RecoveryParams recoveryParams, IImageStorage storage,
        RecoveryIndexImage indexImage)
    {
        var processedImage = _scanningContext.CreateProcessedImage(storage, indexImage.BitDepth.ToBitDepth(),
            indexImage.HighQuality, -1, indexImage.TransformList!.ToImmutableList());

        // TODO: Make this take a lazy rendered image or something
        processedImage = _importPostProcessor.AddPostProcessingData(processedImage,
            _scanningContext.ImageContext.Render(processedImage),
            recoveryParams.ThumbnailSize,
            new BarcodeDetectionOptions(),
            true);
        return processedImage;
    }
}