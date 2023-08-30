using NAPS2.Ocr;

namespace NAPS2.Pdf;

public interface IPdfExporter
{
    Task<bool> Export(string path, ICollection<ProcessedImage> images, PdfExportParams? exportParams = null,
        OcrParams? ocrParams = null, ProgressHandler progress = default);
}