using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Ocr;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.ImportExport.Pdf
{
    public interface IPdfExporter
    {
        Task<bool> Export(string path, ICollection<ScannedImage.Snapshot> snapshots, PdfSettings settings, OcrParams ocrParams, ProgressHandler progressCallback, CancellationToken cancelToken);
    }
}
