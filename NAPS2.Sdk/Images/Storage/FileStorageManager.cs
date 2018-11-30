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

        public virtual string NextFilePath() => Path.Combine(Paths.Temp, Path.GetRandomFileName());
    }
}
