namespace NAPS2.Images;

public interface IPdfRenderer
{
    IEnumerable<IMemoryImage> Render(ImageContext imageContext, string path, float dpi);
}