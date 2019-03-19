using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class FileStorageManager
    {
        private static FileStorageManager _current = new FileStorageManager();

        public static FileStorageManager Current
        {
            get => _current;
            set => _current = value ?? throw new ArgumentNullException(nameof(value));
        }

        public FileStorageManager() : this(Paths.Temp)
        {
        }

        public FileStorageManager(string folderPath)
        {
            FolderPath = folderPath;
        }

        protected string FolderPath { get; }

        public virtual string NextFilePath() => Path.Combine(FolderPath, Path.GetRandomFileName());
    }
}
