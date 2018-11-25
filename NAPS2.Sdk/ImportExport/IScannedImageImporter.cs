using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport
{
    public interface IScannedImageImporter
    {
        ScannedImageSource Import(string filePath, ImportParams importParams, ProgressHandler progressCallback, CancellationToken cancelToken);
    }
}
