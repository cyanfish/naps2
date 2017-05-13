using System;
using System.Collections.Generic;
using System.Linq;
using NAPS2.Scan.Images;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfExporter
    {
        bool Export(string path, IEnumerable<ScannedImage> images, PdfSettings settings, string ocrLanguageCode, Func<int, bool> progressCallback);
    }
}
