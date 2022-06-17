namespace NAPS2.Ocr;

/// <summary>
/// A record that identifies a request in an OcrRequestQueue. When two of these are equal, the OCR requests are
/// considered duplicates.
/// </summary>
internal record OcrRequestParams(ProcessedImage ProcessedImage, IOcrEngine Engine, OcrParams OcrParams);