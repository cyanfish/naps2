using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Scan.Images.Storage
{
    public class PdfFileStorage : IStorage
    {
        public PdfFileStorage(string fullPath)
        {
            FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        }

        public string FullPath { get; }

        public void Dispose()
        {
            try
            {
                File.Delete(FullPath);
            }
            catch (IOException)
            {
            }
        }
    }
}
