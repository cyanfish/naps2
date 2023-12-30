using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

/// <summary>
/// Interface for OCR (optical character recognition). See TesseractOcrEngine.
/// </summary>
public interface IOcrEngine
{
    Task<OcrResult?> ProcessImage(ScanningContext scanningContext, string imagePath, OcrParams ocrParams,
        CancellationToken cancelToken);
}