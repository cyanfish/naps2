using NAPS2.Images.Gdi;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Scan;

namespace NAPS2.Sdk.Samples;

public class OcrSample
{
    public static async Task OcrAndExportPdf()
    {
        // Exporting PDFs with OCR requires the optional NAPS2.Tesseract.Binaries Nuget package to be installed.
        // Or, alternatively, you can use the system-installed Tesseract or provide a custom path to a Tesseract EXE.

        using var scanningContext = new ScanningContext(new GdiImageContext());

        // The NAPS2.Tesseract.Binaries package doesn't include all the actual language data (1GB+ for 100+ languages).
        // You can download .traineddata files from one of these repos:
        // - https://github.com/tesseract-ocr/tessdata_fast
        // - https://github.com/tesseract-ocr/tessdata_best
        // Then specify the folder where those .traineddata files are stored.
        scanningContext.OcrEngine = TesseractOcrEngine.Bundled(@"C:\path\to\my\traineddata\files\");

        // Or if you know Tesseract is installed on the system PATH you can just do this without needing any extra
        // packages or downloads.
        scanningContext.OcrEngine = TesseractOcrEngine.System();

        // Or if you have a custom path to the tesseract EXE you can do this.
        scanningContext.OcrEngine = TesseractOcrEngine.Custom(@"C:\path\to\tesseract.exe");

        // Scan some images
        var controller = new ScanController(scanningContext);
        var devices = await controller.GetDeviceList();
        var options = new ScanOptions { Device = devices.First() };
        var images = await controller.Scan(options).ToListAsync();

        // Export to PDF with OCR
        var pdfExporter = new PdfExporter(scanningContext);
        // We specify the language code for OCR. This is based on the name of the .traineddata file, and is found here:
        // https://tesseract-ocr.github.io/tessdoc/Data-Files#data-files-for-version-400-november-29-2016
        await pdfExporter.Export("doc.pdf", images, ocrParams: new OcrParams("eng"));
    }
}