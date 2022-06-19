using System.Threading;
using NAPS2.Ocr;

namespace NAPS2.ImportExport.Pdf;

public abstract class PdfExporter
{
    // TODO: Unify lifetime management
    // TODO: Also does it make sense to have some kind of custom renderable image collection that encapsulates whether it should dispose on completion?
    public abstract Task<bool> Export(string path, ICollection<ProcessedImage> images, PdfExportParams exportParams,
        OcrParams? ocrParams = null, ProgressHandler? progressCallback = null, CancellationToken cancelToken = default);
}