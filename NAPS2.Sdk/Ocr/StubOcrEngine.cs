using System.Threading;
using NAPS2.Scan;

namespace NAPS2.Ocr;

public class StubOcrEngine : IOcrEngine
{
    public Task<OcrResult?> ProcessImage(ScanningContext scanningContext, string imagePath, OcrParams ocrParams,
        CancellationToken cancelToken)
    {
        return Task.FromResult<OcrResult?>(null);
    }
}