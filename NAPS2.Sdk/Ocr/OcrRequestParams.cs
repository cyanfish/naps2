namespace NAPS2.Ocr;

public record OcrRequestParams(RenderableImage RenderableImage, IOcrEngine Engine, OcrParams OcrParams);