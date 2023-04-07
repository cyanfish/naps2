namespace NAPS2.Ocr;

public class OcrErrorEventArgs
{
    public OcrErrorEventArgs(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
}