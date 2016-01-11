using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<ScannedImage> Import(string filePath, Func<int, int, bool> progressCallback);
    }
}
