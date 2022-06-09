namespace NAPS2.Ocr;

public record OcrRequestParams(ProcessedImage ProcessedImage, IOcrEngine Engine, OcrParams OcrParams);