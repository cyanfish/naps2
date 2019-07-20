using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NAPS2.Images.Transforms;
using NAPS2.Recovery;
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

        private readonly ISerializer<RecoveryIndex> serializer = new XmlSerializer<RecoveryIndex>();
        private readonly DirectoryInfo folder;
        private readonly FileInfo folderLockFile;
        private readonly Stream folderLock;

        private bool disposed;

        public static RecoveryStorageManager CreateFolder(string recoveryFolderPath)
        {
            return new RecoveryStorageManager(recoveryFolderPath);
        }

        private RecoveryStorageManager(string recoveryFolderPath) : base(recoveryFolderPath)
        {
            folder = new DirectoryInfo(RecoveryFolderPath);
            folder.Create();
            folderLockFile = new FileInfo(Path.Combine(RecoveryFolderPath, LOCK_FILE_NAME));
            folderLock = folderLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
        }

        public string RecoveryFolderPath => FolderPath;

        public RecoveryIndex RecoveryIndex { get; } = new RecoveryIndex();

        public void Commit()
        {
            // TODO: Can maybe just lock on one of these, everywhere
            lock (this)
            lock (RecoveryIndex)
            {
                if (disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
                serializer.SerializeToFile(Path.Combine(RecoveryFolderPath, "index.xml"), RecoveryIndex);
            }
        }

        public IImageMetadata CreateMetadata(IStorage storage)
        {
            if (!(storage is FileStorage fileStorage))
            {
                throw new ArgumentException("RecoveryStorageManager can only used with IFileStorage.");
            }
            lock (this)
            lock (RecoveryIndex)
            {
                if (disposed) throw new ObjectDisposedException(nameof(RecoveryStorageManager));
                return new RecoverableImageMetadata(this, new RecoveryIndexImage
                {
                    FileName = Path.GetFileName(fileStorage.FullPath),
                    TransformList = new List<Transform>()
                });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing) return;
            lock (this)
            lock (RecoveryIndex)
            {
                folderLock.Close();
                folderLockFile.Delete();
                folder.Delete(true);
                disposed = true;
            }
        }
    }
}
