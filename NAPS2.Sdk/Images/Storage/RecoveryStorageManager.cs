using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Images.Transforms;
using NAPS2.Recovery;

namespace NAPS2.Images.Storage
{
    public class RecoveryStorageManager : FileStorageManager, IImageMetadataFactory
    {
        public const string LOCK_FILE_NAME = ".lock";

        private int fileNumber;
        private bool folderCreated;
        private FileInfo folderLockFile;
        private Stream folderLock;
        private ConfigManager<RecoveryIndex> indexConfigManager;

        public RecoveryStorageManager(string recoveryFolderPath)
        {
            RecoveryFolderPath = recoveryFolderPath;
        }

        public string RecoveryFolderPath { get; }

        public bool DisableRecoveryCleanup { get; set; }

        public RecoveryIndex Index
        {
            get
            {
                EnsureFolderCreated();
                return indexConfigManager.Config;
            }
        }

        public override string NextFilePath()
        {
            EnsureFolderCreated();
            string fileName = $"{Process.GetCurrentProcess().Id}_{(++fileNumber).ToString("D5", CultureInfo.InvariantCulture)}";
            return Path.Combine(RecoveryFolderPath, fileName);
        }

        private void EnsureFolderCreated()
        {
            if (!folderCreated)
            {
                var folder = new DirectoryInfo(RecoveryFolderPath);
                folder.Create();
                folderLockFile = new FileInfo(Path.Combine(RecoveryFolderPath, LOCK_FILE_NAME));
                folderLock = folderLockFile.Open(FileMode.CreateNew, FileAccess.Write, FileShare.None);
                indexConfigManager = new ConfigManager<RecoveryIndex>("index.xml", RecoveryFolderPath, null, RecoveryIndex.Create);
                folderCreated = true;
            }
        }

        public void Commit()
        {
            // TODO: Clean up when all contents are removed

            EnsureFolderCreated();
            indexConfigManager.Save();
        }

        public IImageMetadata CreateMetadata(IStorage storage)
        {
            var fileStorage = storage as IFileStorage;
            if (fileStorage == null)
            {
                throw new ArgumentException("RecoveryStorageManager can only used with IFileStorage.");
            }
            return new RecoverableImageMetadata(this, new RecoveryIndexImage
            {
                FileName = Path.GetFileName(fileStorage.FullPath),
                TransformList = new List<Transform>()
            });
        }
    }
}
