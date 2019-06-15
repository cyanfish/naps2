using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Storage
{
    /// <summary>
    /// Represents an image received across the wire where we must copy the backing
    /// file before it can be used.
    /// </summary>
    public class UnownedFileStorage : IStorage
    {
        public UnownedFileStorage(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string FilePath { get; }

        public void Dispose()
        {
        }
    }
}
