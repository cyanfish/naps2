using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class FileStorageManager
    {
        private static FileStorageManager _default = new FileStorageManager();

        public static FileStorageManager Default
        {
            get => _default;
            set => _default = value ?? throw new ArgumentNullException(nameof(value));
        }

        public virtual string NextFilePath() => Path.Combine(Paths.Temp, Path.GetRandomFileName());
    }
}
