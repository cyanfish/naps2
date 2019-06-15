using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class OwnershipConverters
    {
        private readonly ImageContext imageContext;

        public OwnershipConverters(ImageContext imageContext)
        {
            this.imageContext = imageContext;
        }

        [StorageConverter]
        public FileStorage ConvertToFile(UnownedFileStorage input, StorageConvertParams convertParams)
        {
            string newPath = imageContext.FileStorageManager.NextFilePath();
            File.Copy(input.FilePath, newPath);
            return new FileStorage(newPath);
        }
    }
}
