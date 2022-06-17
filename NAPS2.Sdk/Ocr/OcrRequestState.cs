namespace NAPS2.Ocr;

/// <summary>
/// The state of the OcrRequest.
/// </summary>
internal enum OcrRequestState
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Error
}