using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfBenchmarkTests : ContextualTests
{
    [Fact(Skip = "run manually")]
    public async Task PdfSharpExport100()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image), BitDepth.Color, true, -1, Enumerable.Empty<Transform>());
    
        var pdfExporter = new PdfSharpExporter(ScanningContext);
        for (int i = 0; i < 100; i++)
        {
            await pdfExporter.Export(filePath + i + ".pdf", new[] { image }, new PdfExportParams());
        }
    }
    
    [Fact(Skip = "run manually")]
    public async Task PdfiumExport100()
    {
        var filePath = Path.Combine(FolderPath, "test");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image), BitDepth.Color, true, -1, Enumerable.Empty<Transform>());
    
        var pdfExporter = new PdfiumPdfExporter(ScanningContext);
        for (int i = 0; i < 100; i++)
        {
            await pdfExporter.Export(filePath + i + ".pdf", new[] { image }, new PdfExportParams());
        }
    }
}