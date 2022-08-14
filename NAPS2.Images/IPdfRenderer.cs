namespace NAPS2.Images;

public interface IPdfRenderer
{
    IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, PdfRenderSize renderSize, string? password = null);

    IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length, PdfRenderSize renderSize,
        string? password = null);
}