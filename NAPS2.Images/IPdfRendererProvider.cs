namespace NAPS2.Images;

internal interface IPdfRendererProvider
{
    IPdfRenderer PdfRenderer { get; }
}