using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class OwnershipConverters
    {
        [StorageConverter]
        public FileStorage ConvertToFile(UnownedFileStorage input, StorageConvertParams convertParams)
        {
            string newPath = FileStorageManager.Current.NextFilePath();
            File.Copy(input.FilePath, newPath);
            return new FileStorage(newPath);
        }
    }
}
