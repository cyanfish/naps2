using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;

namespace NAPS2.Images.Storage
{
    public class PdfConverters
    {
        [StorageConverter]
        public FileStorage ConvertToFile(PdfStorage input, StorageConvertParams convertParams)
        {
            var path = convertParams.Temporary
                ? Path.Combine(Paths.Temp, Path.GetRandomFileName())
                : FileStorageManager.Current.NextFilePath() + ".pdf";
            input.Document.Save(path);
            return new FileStorage(path);
        }

        [StorageConverter]
        public PdfStorage ConvertToMemory(FileStorage input, StorageConvertParams convertParams) => new PdfStorage(new PdfDocument(input.FullPath));
    }
}
