using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NAPS2.Images.Transforms;
using NAPS2.Recovery;
using NAPS2.Serialization;

namespace NAPS2.Images.Storage
{
    // TODO: Locking needs a lot of work.
    public class RecoveryStorageManager : FileStorageManager, IImageMetadataFactory
    {
        public const string LOCK_FILE_NAME = ".lock";

        private readonly ISerializer<RecoveryIndex> serializer = new XmlSerializer<RecoveryIndex>();

        private int fileNumber;
        private bool folderCreated;
        private FileInfo folderLockFile;
        private Stream folderLock;
        private RecoveryIndex recoveryIndex;

        public RecoveryStorageManager(string recoveryFolderPath, bool skipCreate = false) : base(recoveryFolderPath)
        {
            folderCreated = skipCreate;
        }

        public string RecoveryFolderPath => FolderPath;

        public bool DisableRecoveryCleanup { get; set; }

        public RecoveryIndex Index
        {
            get
            {
                EnsureFolderCreated();
                return recoveryIndex;
            }
        }

        public override string NextFilePath()
        {
            lock (this)
            {
                EnsureFolderCreated();
                string fileName = $"{Process.GetCurrentProcess().Id}_{(++fileNumber).ToString("D5", CultureInfo.InvariantCulture)}";
                return Path.Combine(RecoveryFolderPath, fileName);
            }
        }

        public void EnsureFolderCreated()
        {
            lock (this)
            {
                if (!folderCreated)
                {
                    var folder = new DirectoryInfo(RecoveryFolderPath);
                    folder.Create();
                    folderLockFile = new FileInfo(Path.Combine(RecoveryFolderPath, LOCK_FILE_NAME));
                    folderLock = folderLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
                    recoveryIndex = new RecoveryIndex();
                    folderCreated = true;
                }
            }
        }

        public void Commit()
        {
            lock (this)
            {
                EnsureFolderCreated();
                if (recoveryIndex.Images.Count == 0)
                {
                    // Clean up
                    ForceReleaseLock();
                    Directory.Delete(RecoveryFolderPath, true);
                    recoveryIndex = null;
                    folderCreated = false;
                }
                else
                {
                    serializer.SerializeToFile(Path.Combine(RecoveryFolderPath, "index.xml"), recoveryIndex);
                }
            }
        }

        public IImageMetadata CreateMetadata(IStorage storage)
        {
            if (!(storage is FileStorage fileStorage))
            {
                throw new ArgumentException("RecoveryStorageManager can only used with IFileStorage.");
            }
            return new RecoverableImageMetadata(this, new RecoveryIndexImage
            {
                FileName = Path.GetFileName(fileStorage.FullPath),
                TransformList = new List<Transform>()
            });
        }

        public void ForceReleaseLock()
        {
            lock (this)
            {
                folderLock?.Close();
                folderLockFile?.Delete();
                folderLock = null;
                folderLockFile = null;
            }
        }
    }
}
