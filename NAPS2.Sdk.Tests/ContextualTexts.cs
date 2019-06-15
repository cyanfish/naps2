using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Config;
using NAPS2.Images.Storage;

namespace NAPS2.Sdk.Tests
{
    public class ContextualTexts : IDisposable
    {
        private RecoveryStorageManager rsm;

        public ContextualTexts()
        {
            FolderPath = $"naps2_test_temp_{Path.GetRandomFileName()}";
            Folder = Directory.CreateDirectory(FolderPath);
            var tempPath = Path.Combine(FolderPath, "temp");
            Directory.CreateDirectory(tempPath);

            ProfileManager.Current = new StubProfileManager();

            ImageContext = new GdiImageContext
            {
                FileStorageManager = new FileStorageManager(tempPath)
            };
        }

        public ImageContext ImageContext { get; }

        public string FolderPath { get; }

        public DirectoryInfo Folder { get; }

        public void UseFileStorage()
        {
            ImageContext.ConfigureBackingStorage<FileStorage>();
        }

        public void UseRecovery()
        {
            var recoveryFolderPath = Path.Combine(FolderPath, "recovery", Path.GetRandomFileName());
            rsm = new RecoveryStorageManager(recoveryFolderPath);
            ImageContext.FileStorageManager = rsm;
            ImageContext.ConfigureBackingStorage<FileStorage>();
            ImageContext.ImageMetadataFactory = rsm;
        }

        public virtual void Dispose()
        {
            rsm?.ForceReleaseLock();
            Directory.Delete(FolderPath, true);
        }
    }
}
