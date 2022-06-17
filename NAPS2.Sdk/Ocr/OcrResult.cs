namespace NAPS2.Ocr;

/// <summary>
/// The result of an OCR request. Contains a set of elements that represent text segments. 
/// </summary>
public class OcrResult
{
    public OcrResult((int x, int y, int w, int h) pageBounds, IEnumerable<OcrResultElement> elements, bool rightToLeft)
    {
        PageBounds = pageBounds;
        Elements = elements;
        RightToLeft = rightToLeft;
    }

    public (int x, int y, int w, int h) PageBounds { get; }

    public IEnumerable<OcrResultElement> Elements { get; }

    public bool RightToLeft { get; }
}