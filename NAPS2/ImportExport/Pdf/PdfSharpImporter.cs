using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Pdf
{
    public class PdfSharpImporter : IScannedImageImporter
    {
        public IEnumerable<IScannedImage> Import(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
