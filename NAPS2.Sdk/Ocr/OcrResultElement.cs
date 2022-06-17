namespace NAPS2.Ocr;

/// <summary>
/// A element in the result of an OCR request that represents a text segment.
/// </summary>
public class OcrResultElement
{
    public OcrResultElement(string text, (int x, int y, int w, int h) bounds)
    {
        Text = text;
        Bounds = bounds;
    }

    public string Text { get; }
        
    public (int x, int y, int w, int h) Bounds { get; }
}