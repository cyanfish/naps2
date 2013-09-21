using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<IScannedImage> Import(string filePath);
    }
}
