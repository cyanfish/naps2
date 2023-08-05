using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

internal class StubOcrEngine : IOcrEngine
{
    public Task<OcrResult?> ProcessImage(ScanningContext scanningContext, string imagePath, OcrParams ocrParams,
        CancellationToken cancelToken)
    {
        return Task.FromResult<OcrResult?>(null);
    }
}