using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Pdf;

namespace NAPS2.Images.Storage
{
    public class PdfStorage : IStorage
    {
        static PdfStorage()
        {
            StorageManager.RegisterConverters(new PdfConverters());
        }

        public PdfStorage(PdfDocument document)
        {
            Document = document ?? throw new ArgumentNullException(nameof(document));
        }

        public PdfDocument Document { get; }

        public void Dispose()
        {
        }
    }
}
