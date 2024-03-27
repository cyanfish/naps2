using System.Collections.Immutable;

namespace NAPS2.Ocr;

/// <summary>
/// The result of an OCR request. Contains a set of elements that represent text segments. 
/// </summary>
public class OcrResult(
    (int x, int y, int w, int h) pageBounds,
    ImmutableList<OcrResultElement> words,
    ImmutableList<OcrResultElement> lines)
{
    public (int x, int y, int w, int h) PageBounds { get; } = pageBounds;

    public ImmutableList<OcrResultElement> Words { get; } = words;

    public ImmutableList<OcrResultElement> Lines { get; } = lines;
}