using NAPS2.Ocr;

namespace NAPS2.ImportExport.Pdf;

public interface IPdfExporter
{
    Task<bool> Export(string path, ICollection<ProcessedImage> images, PdfExportParams exportParams,
        OcrParams? ocrParams = null, ProgressHandler progress = default);
}