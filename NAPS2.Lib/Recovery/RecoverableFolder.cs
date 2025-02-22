using System.Collections.Immutable;
using NAPS2.ImportExport;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Recovery;

public class RecoverableFolder : IDisposable
{
    private readonly ScanningContext _scanningContext;
    private readonly DirectoryInfo _directory;
    private readonly FileStream _lockFile;
    private readonly RecoveryIndex _recoveryIndex;
    private bool _disposed;

    public static RecoverableFolder? TryCreate(ScanningContext scanningContext, DirectoryInfo directory)
    {
        string indexFilePath = Path.Combine(directory.FullName, RecoveryStorageManager.INDEX_FILE_NAME);
        string lockFilePath = Path.Combine(directory.FullName, RecoveryStorageManager.LOCK_FILE_NAME);
        if (!File.Exists(lockFilePath))
        {
            MaybeCleanUp(directory);
            return null;
        }
        var lockFile = new FileStream(lockFilePath, FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            var serializer = new XmlSerializer<RecoveryIndex>();
            var recoveryIndex = serializer.DeserializeFromFile(indexFilePath);
            var imageCount = recoveryIndex.Images.Count;
            var scannedDateTime = directory.LastWriteTime;
            if (imageCount == 0)
            {
                lockFile.Dispose();
                MaybeCleanUp(directory);
                return null;
            }
            return new RecoverableFolder(scanningContext, directory, lockFile, recoveryIndex,
                imageCount, scannedDateTime);
        }
        catch (Exception)
        {
            lockFile.Dispose();
            MaybeCleanUp(directory);
            return null;
        }
    }

    private static void MaybeCleanUp(DirectoryInfo directory)
    {
        // Clean up empty folders immediately, folders with files after a week
        var cutoff = DateTime.Now - TimeSpan.FromDays(7);
        var files = directory.GetFiles();
        if (files.Any(x =>
                x.LastWriteTime > cutoff &&
                x.Name != RecoveryStorageManager.LOCK_FILE_NAME &&
                x.Name != RecoveryStorageManager.INDEX_FILE_NAME))
        {
            return;
        }
        try
        {
            directory.Delete(true);
        }
        catch (IOException)
        {
        }
    }

    public RecoverableFolder(ScanningContext scanningContext, DirectoryInfo directory, FileStream lockFile,
        RecoveryIndex recoveryIndex, int imageCount, DateTime scannedDateTime)
    {
        _scanningContext = scanningContext;
        _directory = directory;
        _lockFile = lockFile;
        _recoveryIndex = recoveryIndex;
        ImageCount = imageCount;
        ScannedDateTime = scannedDateTime;
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

    private Dictionary<string, StorageKey> CreateStorageKeys()
    {
        return _recoveryIndex.Images
            .Select(indexImage => indexImage.FileName)
            .Distinct()
            .Select(originalFileName => new StorageKey(_scanningContext, _directory, originalFileName!))
            .ToDictionary(x => x.OriginalFileName);
    }

    public IEnumerable<ProcessedImage> FastRecover()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RecoverableFolder));

        var storageKeys = CreateStorageKeys();
        var recoveredImages = new List<ProcessedImage>();
        foreach (RecoveryIndexImage indexImage in _recoveryIndex.Images)
        {
            var storageKey = storageKeys[indexImage.FileName!];
            storageKey.EnsureMigrated(move: true);
            if (storageKey.NewPath != null)
            {
                recoveredImages.Add(CreateRecoveredImage(new RecoveryParams(), indexImage, storageKey));
            }
        }

        // Delete the old folder (which should be mostly empty now)
        TryDelete();
        return recoveredImages;
    }

    public bool TryRecover(Action<ProcessedImage> imageCallback, RecoveryParams recoveryParams,
        ProgressHandler progress)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RecoverableFolder));

        int currentProgress = 0;
        int totalProgress = ImageCount;
        progress.Report(currentProgress, totalProgress);

        var storageKeys = CreateStorageKeys();
        foreach (var indexImage in _recoveryIndex.Images)
        {
            if (progress.IsCancellationRequested)
            {
                return false;
            }
            var storageKey = storageKeys[indexImage.FileName!];
            storageKey.EnsureMigrated(move: false);
            if (storageKey.NewPath != null)
            {
                var recoveredImage = CreateRecoveredImage(recoveryParams, indexImage, storageKey);
                imageCallback(recoveredImage);
            }
            currentProgress++;
            progress.Report(currentProgress, totalProgress);
        }

        // Now that we've recovered successfully, we can safely delete the old folder
        TryDelete();
        return true;
    }

    private ProcessedImage CreateRecoveredImage(RecoveryParams recoveryParams, RecoveryIndexImage indexImage,
        StorageKey storageKey)
    {
        var processedImage = _scanningContext.CreateProcessedImage(
            storageKey.CreateStorage(),
            indexImage.HighQuality,
            -1,
            PageSize.Parse(indexImage.PageSize),
            indexImage.TransformList!.ToImmutableList(),
            storageKey.RefCount);
        storageKey.RefCount ??= processedImage.StorageRefCount;

        // TODO: Make this take a lazy rendered image or something
        processedImage = ImportPostProcessor.AddPostProcessingData(processedImage,
            null,
            recoveryParams.ThumbnailSize,
            new BarcodeDetectionOptions(),
            true);
        return processedImage;
    }

    private record StorageKey(
        ScanningContext ScanningContext,
        DirectoryInfo OriginalDirectory,
        string OriginalFileName)
    {
        public void EnsureMigrated(bool move)
        {
            if (Migrated) return;
            Migrated = true;
            try
            {
                var newPath = GetNewPath();
                if (move)
                {
                    File.Move(OriginalPath, newPath);
                }
                else
                {
                    File.Copy(OriginalPath, newPath);
                }
                NewPath = newPath;
            }
            catch (Exception e)
            {
                Log.ErrorException("Could not recover image", e);
            }
        }

        public string OriginalPath => Path.Combine(OriginalDirectory.FullName, OriginalFileName);

        public string? NewPath { get; private set; }

        public bool Migrated { get; private set; }

        public IImageStorage? Storage { get; private set; }

        public RefCount? RefCount { get; set; }

        public IImageStorage CreateStorage() =>
            Storage ??= new ImageFileStorage(NewPath ?? throw new InvalidOperationException());

        private string GetNewPath() =>
            ScanningContext.FileStorageManager!.NextFilePath() + Path.GetExtension(OriginalFileName);
    }
}