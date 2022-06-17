namespace NAPS2.Ocr;

internal enum OcrRequestState
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Error
}