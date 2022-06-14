namespace NAPS2.Ocr;

public class OcrEventArgs : EventArgs
{
    public OcrEventArgs(Task<OcrResult?> resultTask)
    {
        ResultTask = resultTask;
    }

    public Task<OcrResult?> ResultTask { get; }
}