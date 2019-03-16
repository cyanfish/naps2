using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class OwnershipConverters
    {
        [StorageConverter]
        public IFileStorage ConvertToFile(UnownedTransferStorage input, StorageConvertParams convertParams)
        {
            string newPath = FileStorageManager.Current.NextFilePath();
            File.Copy(input.FilePath, newPath);
            if (".pdf".Equals(Path.GetExtension(input.FilePath), StringComparison.InvariantCultureIgnoreCase))
            {
                return new PdfFileStorage(newPath);
            }
            else
            {
                return new FileStorage(newPath);
            }
        }

        [StorageConverter]
        public IFileStorage ConvertToFile(OwnedTransferStorage input, StorageConvertParams convertParams)
        {
            // TODO: Maybe verify that it's in the right folder and move otherwise?
            if (".pdf".Equals(Path.GetExtension(input.FilePath), StringComparison.InvariantCultureIgnoreCase))
            {
                return new PdfFileStorage(input.FilePath);
            }
            else
            {
                return new FileStorage(input.FilePath);
            }
        }
    }
}
