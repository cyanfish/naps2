namespace NAPS2.Images;

public class PdfRenderSize
{
    public static PdfRenderSize FromDpi(float dpi)
    {
        return new PdfRenderSize { Dpi = dpi };
    }

    public static PdfRenderSize FromDimensions(int width, int height)
    {
        return new PdfRenderSize { Width = width, Height = height };
    }

    private PdfRenderSize()
    {
    }
    
    public float? Dpi { get; private init; }
    
    public int? Width { get; private init; }
    
    public int? Height { get; private init; }
}