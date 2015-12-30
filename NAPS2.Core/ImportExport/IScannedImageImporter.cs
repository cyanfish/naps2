using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<IScannedImage> Import(string filePath, Func<int, int, bool> progressCallback);
    }
}
