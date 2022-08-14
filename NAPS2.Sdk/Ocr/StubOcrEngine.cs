using System.Threading;

namespace NAPS2.Ocr;

public class StubOcrEngine : IOcrEngine
{
    public Task<OcrResult?> ProcessImage(string imagePath, OcrParams ocrParams, CancellationToken cancelToken)
    {
        return Task.FromResult<OcrResult?>(null);
    }
}