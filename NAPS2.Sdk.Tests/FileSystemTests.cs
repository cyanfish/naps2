using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAPS2.Images.Storage;

namespace NAPS2.Sdk.Tests
{
    public class FileSystemTests : IDisposable
    {
        public FileSystemTests()
        {
            FolderPath = $"naps2_test_temp_{Path.GetRandomFileName()}";
            Folder = Directory.CreateDirectory(FolderPath);
            FileStorageManager.Current = new FileStorageManager(FolderPath);
        }

        public string FolderPath { get; }

        public DirectoryInfo Folder { get; }

        public void Dispose()
        {
            Directory.Delete(FolderPath, true);
        }
    }
}
