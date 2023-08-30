using NAPS2.Images.Gdi;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class ScanAndSaveSample
{
    public static async Task ScanAndSave()
    {
        // Set up
        using var scanningContext = new ScanningContext(new GdiImageContext());
        var controller = new ScanController(scanningContext);

        // Query for available scanning devices
        var devices = await controller.GetDeviceList();

        // Set scanning options
        var options = new ScanOptions
        {
            Device = devices.First(),
            PaperSource = PaperSource.Feeder,
            PageSize = PageSize.A4,
            Dpi = 300
        };

        // Scan and save images
        int i = 1;
        await foreach (var image in controller.Scan(options))
        {
            image.Save($"page{i++}.jpg");
        }

        // Scan and save PDF
        var images = await controller.Scan(options).ToListAsync();
        var pdfExporter = new PdfExporter(scanningContext);
        await pdfExporter.Export("doc.pdf", images);
    }
}