using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<ScannedImage> Import(string filePath, ImportParams importParams, Func<int, int, bool> progressCallback);
    }
}
