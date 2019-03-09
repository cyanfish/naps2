using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Sdk.Tests
{
    public class FileSystemTests : IDisposable
    {
        public FileSystemTests()
        {
            FolderPath = $"temp_{Path.GetRandomFileName()}";
            Folder = Directory.CreateDirectory(FolderPath);
        }

        public string FolderPath { get; }

        public DirectoryInfo Folder { get; }

        public void Dispose()
        {
            Directory.Delete(FolderPath);
        }
    }
}
