namespace NAPS2.Images;

public interface IPdfRendererProvider
{
    IPdfRenderer PdfRenderer { get; }
}