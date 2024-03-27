using System.Collections.Immutable;

namespace NAPS2.Ocr;

/// <summary>
/// A element in the result of an OCR request that represents a text segment.
/// </summary>
public record OcrResultElement(
    string Text,
    string LanguageCode,
    bool RightToLeft,
    (int x, int y, int w, int h) Bounds,
    int Baseline,
    int FontSize,
    ImmutableList<OcrResultElement> Children);