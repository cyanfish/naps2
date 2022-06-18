using System.Threading;

namespace NAPS2.Ocr;

public interface IOcrEngine
{
    Task<OcrResult?> ProcessImage(string imagePath, OcrParams ocrParams, CancellationToken cancelToken);
}