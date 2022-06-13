using System.Threading;

namespace NAPS2.Ocr;

internal class OcrRequest
{
    public OcrRequest(OcrRequestParams reqParams)
    {
        Params = reqParams;
    }

    public OcrRequestParams Params { get; }

    public string? TempImageFilePath { get; set; }

    public CancellationTokenSource CancelSource { get; } = new CancellationTokenSource();

    public ManualResetEvent WaitHandle { get; } = new ManualResetEvent(false);

    public bool IsProcessing { get; set; }

    public OcrResult? Result { get; set; }

    public Dictionary<OcrPriority, int> PriorityRefCount { get; } = new()
    {
        {OcrPriority.Foreground, 0},
        {OcrPriority.Background, 0}
    };

    public bool HasLiveReference =>
        PriorityRefCount[OcrPriority.Foreground] > 0 || PriorityRefCount[OcrPriority.Background] > 0;
}