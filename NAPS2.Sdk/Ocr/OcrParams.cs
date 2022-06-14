namespace NAPS2.Ocr;

public record OcrParams(string? LanguageCode, OcrMode Mode, double TimeoutInSeconds);