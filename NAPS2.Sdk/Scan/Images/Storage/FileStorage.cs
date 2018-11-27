using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class FileStorage : IStorage
    {
        private readonly FileStorageManager fileStorageManager;

        public FileStorage(FileStorageManager fileStorageManager, string fullPath)
        {
            this.fileStorageManager = fileStorageManager;
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        }

        public string FullPath { get; }

        public void Dispose()
        {
            try
            {
                File.Delete(FullPath);
                fileStorageManager.Detach(FullPath);
            }
            catch (IOException)
            {
            }
        }
    }
}
