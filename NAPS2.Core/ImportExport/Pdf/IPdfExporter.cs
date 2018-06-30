using NAPS2.Scan.Images;
using System;
using System.Collections.Generic;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfExporter
    {
        bool Export(string path, IEnumerable<ScannedImage> images, PdfSettings settings, string ocrLanguageCode, Func<int, bool> progressCallback);
    }
}