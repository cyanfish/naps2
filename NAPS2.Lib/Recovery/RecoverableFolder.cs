using System.Collections.Immutable;
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
        _lockFile = new FileStream(lockFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            var serializer = new XmlSerializer<RecoveryIndex>();
            _recoveryIndex = serializer.DeserializeFromFile(Path.Combine(directory.FullName, "index.xml"));
            ImageCount = _recoveryIndex.Images.Count;
            ScannedDateTime = directory.LastWriteTime;
            // TODO: Consider auto-delete in this case
            // TODO: Also in the case where you have a lock file but no index is written (especially if no images are present)
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
        ProgressHandler progress)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RecoverableFolder));

        int currentProgress = 0;
        int totalProgress = ImageCount;
        progress.Report(currentProgress, totalProgress);

        foreach (RecoveryIndexImage indexImage in _recoveryIndex.Images)
        {
            if (progress.IsCancellationRequested)
            {
                return false;
            }

            string imagePath = Path.Combine(_directory.FullName, indexImage.FileName!);
            var ext = Path.GetExtension(imagePath);
            string newPath = _scanningContext.FileStorageManager!.NextFilePath() + ext;

            try
            {
                File.Copy(imagePath, newPath);
            }
            catch (Exception e)
            {
                // TODO: Should we treat FileNotFound differently than other exceptions? i.e. continue on FNF, abort otherwise
                Log.ErrorException("Could not recover image", e);

                currentProgress++;
                progress.Report(currentProgress, totalProgress);
                continue;
            }

            var storage = new ImageFileStorage(newPath);
            var recoveredImage = CreateRecoveredImage(recoveryParams, storage, indexImage);
            imageCallback(recoveredImage);

            currentProgress++;
            progress.Report(currentProgress, totalProgress);
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
            null,
            recoveryParams.ThumbnailSize,
            new BarcodeDetectionOptions(),
            true);
        return processedImage;
    }
}