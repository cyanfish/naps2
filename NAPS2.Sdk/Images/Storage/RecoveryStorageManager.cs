using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Recovery;
using NAPS2.Scan;
using NAPS2.Serialization;

namespace NAPS2.Images.Storage
{
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
    public class RecoveryStorageManager : FileStorageManager, IImageMetadataFactory
    {
        public const string LOCK_FILE_NAME = ".lock";

        private readonly ISerializer<RecoveryIndex> _serializer = new XmlSerializer<RecoveryIndex>();
        private readonly DirectoryInfo _folder;
        private readonly FileInfo _folderLockFile;
        private readonly Stream _folderLock;
        private readonly Dictionary<RecoverableImageMetadata, int> _metadataDict = new Dictionary<RecoverableImageMetadata, int>();

        private int _metadataOrdering;
        private bool _disposed;

        public static RecoveryStorageManager CreateFolder(string recoveryFolderPath)
        {
            return new RecoveryStorageManager(recoveryFolderPath);
        }

        private RecoveryStorageManager(string recoveryFolderPath) : base(recoveryFolderPath)
        {
            _folder = new DirectoryInfo(RecoveryFolderPath);
            _folder.Create();
            _folderLockFile = new FileInfo(Path.Combine(RecoveryFolderPath, LOCK_FILE_NAME));
            _folderLock = _folderLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
        }

        public string RecoveryFolderPath => FolderPath;

        public void CommitAllMetadata()
        {
            lock (this)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
                var recoveryIndex = RecoveryIndex.Create();
                var orderedMetadata = _metadataDict.OrderBy(x => x.Key.Index).ThenBy(x => x.Value).Select(x => x.Key);
                recoveryIndex.Images = orderedMetadata.Select(metadata => new RecoveryIndexImage
                {
                    FileName = metadata.FileName,
                    BitDepth = metadata.BitDepth.ToScanBitDepth(),
                    HighQuality = metadata.Lossless,
                    TransformList = metadata.TransformList
                }).ToList();
                _serializer.SerializeToFile(Path.Combine(RecoveryFolderPath, "index.xml"), recoveryIndex);
            }
        }

        public IImageMetadata CreateMetadata(IStorage storage)
        {
            if (!(storage is FileStorage fileStorage))
            {
                throw new ArgumentException("RecoveryStorageManager can only used with IFileStorage.");
            }
            lock (this)
            {
                if (_disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
                return new RecoverableImageMetadata(this, Path.GetFileName(fileStorage.FullPath));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            lock (this)
            {
                _folderLock.Close();
                _folderLockFile.Delete();
                _folder.Delete(true);
                _disposed = true;
            }
        }

        public void RegisterMetadata(RecoverableImageMetadata metadata)
        {
            lock (this)
            {
                _metadataDict.Add(metadata, _metadataOrdering++);
            }
        }

        public void UnregisterMetadata(RecoverableImageMetadata metadata)
        {
            lock (this)
            {
                _metadataDict.Remove(metadata);
            }
        }
    }
}
