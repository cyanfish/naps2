using System;
using System.Collections.Generic;
using System.Linq;
using PdfSharp.Pdf;

namespace NAPS2.Images.Storage
{
    public class PdfStorage : IStorage
    {
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
