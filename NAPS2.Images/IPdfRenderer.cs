namespace NAPS2.Images;

public interface IPdfRenderer
{
    IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float defaultDpi, string? password = null);

    IEnumerable<IMemoryImage> Render(ImageContext imageContext, byte[] buffer, int length, float defaultDpi,
        string? password = null);
}