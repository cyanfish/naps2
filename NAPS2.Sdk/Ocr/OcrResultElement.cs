namespace NAPS2.Ocr;

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