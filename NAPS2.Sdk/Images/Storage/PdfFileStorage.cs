using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAPS2.Images.Storage
{
    public class PdfFileStorage : IFileStorage
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
