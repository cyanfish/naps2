using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images;

/// <summary>
/// Manages the lifetime of a recovery folder.
///
/// "Recovery" means that in the case of a crash (of the application or machine), there is enough information on disk to
/// restore any images that weren't previously disposed.
///
/// From a design perspective, there are several elements to recovery:
/// - The recovery folder. Created in RecoveryStorageManager.CreateFolder and deleted by RecoveryStorageManager.Dispose.
/// - The image files. Created in ScanningContext.CreateProcessedImage based on ScanningContext.FileStorageManager (which should have the same path as the RecoveryStorageManager).
/// - The recovery index. This is file named index.xml which stores the serialized metadata entries and is updated by RecoveryStorageManager.WriteIndex.
/// - The recovery lock. This is a file named .lock that has an exclusive write lock until RecoveryStorageManager is disposed.
///
/// If the process crashes without disposing RecoveryStorageManager, the lock will be automatically released.
/// Other processes can later detect that and initiate a recovery via RecoveryManager. 
/// </summary>
public class RecoveryStorageManager : IDisposable
{
    public const string LOCK_FILE_NAME = ".lock";
    private static readonly TimeSpan WriteThrottleInterval = TimeSpan.FromMilliseconds(100);

    private readonly ISerializer<RecoveryIndex> _serializer = new XmlSerializer<RecoveryIndex>();
    private readonly DirectoryInfo _folder;
    private readonly FileInfo _folderLockFile;
    private readonly Stream _folderLock;
    private readonly TimedThrottle _writeThrottle;
    private UiImageList _imageList;

    private bool _disposed;

    public static RecoveryStorageManager CreateFolder(string recoveryFolderPath)
    {
        return new RecoveryStorageManager(recoveryFolderPath);
    }

    private RecoveryStorageManager(string recoveryFolderPath)
    {
        RecoveryFolderPath = recoveryFolderPath;
        _folder = new DirectoryInfo(RecoveryFolderPath);
        _folder.Create();
        _folderLockFile = new FileInfo(Path.Combine(RecoveryFolderPath, LOCK_FILE_NAME));
        _folderLock = _folderLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
        _writeThrottle = new TimedThrottle(WriteIndexFromImageList, WriteThrottleInterval);
    }

    public string RecoveryFolderPath { get; }

    // TODO: Maybe just make UiImageList part of the constructor?
    public void RegisterForChanges(UiImageList imageList)
    {
        lock (this)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
            if (_imageList != null) throw new InvalidOperationException();
            _imageList = imageList;
            imageList.ImagesUpdated += ImageListUpdated;
            imageList.ImagesThumbnailInvalidated += ImageListUpdated;
        }
    }

    private void ImageListUpdated(object? sender, EventArgs args)
    {
        _writeThrottle.RunAction(null);
    }

    private void WriteIndexFromImageList()
    {
        List<UiImage> images;
        lock (_imageList)
        {
            images = _imageList.Images.ToList();
        }
        WriteIndex(images);
    }
    
    public void WriteIndex(IEnumerable<UiImage> images)
    {
        lock (this)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
            var recoveryIndex = RecoveryIndex.Create();
            recoveryIndex.Images = images.Select(image =>
            {
                using var processedImage = image.GetClonedImage();
                var storage = processedImage.Storage as ImageFileStorage
                              ?? throw new InvalidOperationException("RecoveryStorageManager can only be used with FileStorage");
                return new RecoveryIndexImage
                {
                    FileName = Path.GetFileName(storage.FullPath),
                    BitDepth = processedImage.Metadata.BitDepth.ToScanBitDepth(),
                    HighQuality = processedImage.Metadata.Lossless,
                    TransformList = processedImage.TransformState.Transforms.ToList()
                };
            }).ToList();
            _serializer.SerializeToFile(Path.Combine(RecoveryFolderPath, "index.xml"), recoveryIndex);
        }
    }

    public void ReleaseLockForTesting()
    {
        _folderLock.Close();
    }

    public void Dispose()
    {
        lock (this)
        {
            if (_disposed) return;
            _folderLock.Close();
            _folderLockFile.Delete();
            _folder.Delete(true);
            if (_imageList != null)
            {
                _imageList.ImagesUpdated -= ImageListUpdated;
                _imageList.ImagesThumbnailInvalidated -= ImageListUpdated;
            }
            _disposed = true;
        }
    }
}