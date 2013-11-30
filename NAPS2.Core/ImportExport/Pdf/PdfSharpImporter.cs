using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpImporter : IPdfImporter
    {
        public IEnumerable<IScannedImage> Import(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
