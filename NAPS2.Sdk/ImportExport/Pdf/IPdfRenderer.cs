namespace NAPS2.ImportExport.Pdf;

public interface IPdfRenderer
{
    IEnumerable<IImage> Render(string path, float dpi);
}