using NAPS2.Images.Gdi;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class PdfImportSample
{
    public static async Task PdfImportAppendAndExport()
    {
        // Importing PDFs requires the optional NAPS2.Pdfium.Binaries Nuget package to be installed.

        // Set up
        using var scanningContext = new ScanningContext(new GdiImageContext());
        var pdfImporter = new PdfImporter(scanningContext);
        var images = new List<ProcessedImage>();

        // Import original PDF
        await foreach (var image in pdfImporter.Import("original.pdf"))
        {
            images.Add(image);
        }

        // Set up scanning
        var controller = new ScanController(scanningContext);
        var devices = await controller.GetDeviceList();
        var options = new ScanOptions { Device = devices.First() };

        // Append newly scanned images
        await foreach (var image in controller.Scan(options))
        {
            images.Add(image);
        }

        // Save all images to a new PDF
        // This will retain the structure of the original PDF pages (i.e. they aren't rasterized to flat images)
        var pdfExporter = new PdfExporter(scanningContext);
        await pdfExporter.Export("doc.pdf", images);
    }
}