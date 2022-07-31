using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

public class PdfiumPdfExporterTests : ContextualTests
{
    // TODO: Access violations with 32-bit pdfium. Can change default arch in rider unit testing settings to reproduce.
    // Specifically, LoadJpegFileInline and SaveAsCopy both cause access violations.
    // What they have in common is the pattern of passing in a struct with a function pointer. But the debugger can
    // step into the function being called correctly so idk if that's the actual problem.
    // I already corrected that some variables defined as "long" needed to be IntPtr to be arch-correct.
    // It could be something to to with the return value or calling convention from those functions? But I've already
    // experimented with those to no effect.
    [PlatformFact(require64Bit: true)]
    public async Task ExportSingleImage()
    {
        var filePath = Path.Combine(FolderPath, "test.pdf");
        using var image = ScanningContext.CreateProcessedImage(new GdiImage(ImageResources.color_image));
    
        var pdfExporter = new PdfiumPdfExporter(ScanningContext);
        await pdfExporter.Export(filePath, new[] { image }, new PdfExportParams());
        
        PdfAsserts.AssertImages(filePath, ImageResources.color_image);
    }
}