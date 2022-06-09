namespace NAPS2.ImportExport.Pdf;

public interface IPdfRenderer
{
    IEnumerable<IMemoryImage> Render(string path, float dpi);
}