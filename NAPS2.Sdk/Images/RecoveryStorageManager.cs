using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images;

// TODO: Write tests for this class
/// <summary>
/// Manages the lifetime of a recovery folder.
///
/// "Recovery" means that in the case of a crash (of the application or machine), there is enough information on disk to restore any ScannedImage objects
/// that weren't previously disposed.
///
/// From a design perspective, there are several elements to recovery:
/// - The recovery folder. Created in the RecoveryStorageManager constructor and deleted by ImageContext.Dispose.
/// - The image files. Created when constructing the IFileStorage for ScannedImage and deleted by ScannedImage.Dispose.
/// - The image metadata entry. Created when constructing RecoverableImageMetadata and deleted by ScannedImage.Dispose.
/// - The recovery index. This is file named index.xml which stores the serialized metadata entries.
/// - The recovery lock. This is a file named .lock that is locked until RecoveryStorageManager is disposed. This is used to determine if the application is running or not (in which case you can prompt to recover from disk).
///
/// To use recovery, follow these steps:
/// 1. Call ImageContext.UseRecovery with the path to the folder you want to store the recovery-related files.
/// 2. Scan as usual, creating ScannedImage objects.
/// 3. When you no longer need each ScannedImage, call its Dispose method.
/// 4. When you've disposed all ScannedImages and are done with scanning, call ImageContext.Dispose.
/// 5. If you want to deliberately keep image data to be recovered, don't call Dispose on ScannedImage or ImageContext.
/// 6. See the RecoveryManager doc for how to actually recover ScannedImage data from disk.
/// </summary>
public class RecoveryStorageManager : IDisposable
{
    public const string LOCK_FILE_NAME = ".lock";

    private readonly ISerializer<RecoveryIndex> _serializer = new XmlSerializer<RecoveryIndex>();
    private readonly DirectoryInfo _folder;
    private readonly FileInfo _folderLockFile;
    private readonly Stream _folderLock;

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
    }

    public string RecoveryFolderPath { get; }

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

    public void Dispose()
    {
        lock (this)
        {
            if (_disposed) return;
            _folderLock.Close();
            _folderLockFile.Delete();
            _folder.Delete(true);
            _disposed = true;
        }
    }
}