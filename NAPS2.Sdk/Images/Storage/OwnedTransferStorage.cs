using System;
using System.Collections.Generic;
using System.Linq;

namespace NAPS2.Images.Storage
{
    /// <summary>
    /// Represents an image received across the wire where we can take ownership
    /// of the backing file.
    /// </summary>
    public class OwnedTransferStorage : IStorage
    {
        static OwnedTransferStorage()
        {
            StorageManager.RegisterConverters(new OwnershipConverters());
        }

        public OwnedTransferStorage(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        }

        public string FilePath { get; }

        public void Dispose()
        {
        }
    }
}
