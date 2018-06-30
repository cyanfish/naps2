using NAPS2.Scan.Images;
using System;
using System.Collections.Generic;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        IEnumerable<ScannedImage> Import(string filePath, ImportParams importParams, Func<int, int, bool> progressCallback);
    }
}