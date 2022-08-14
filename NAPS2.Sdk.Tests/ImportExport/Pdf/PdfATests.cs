using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

// TODO: Validate with OCR output
// TODO: Maaaybe validate with external import? We certainly can't guarantee it, but maybe some cases can be verified for best effort
public class PdfATests : ContextualTests
{
    // Sadly the pdfa verifier library only supports windows/mac
    [PlatformFact(exclude: PlatformFlags.Mac)]
    public async Task Validate()
    {
        var pdfExporter = new PdfExporter(ScanningContext);
        var testCases = new (PdfCompat pdfCompat, string profile, string fileName)[]
        {
            (PdfCompat.PdfA1B, "PDF/A-1B", "pdfa1b_test.pdf"),
            (PdfCompat.PdfA2B, "PDF/A-2B", "pdfa2b_test.pdf"),
            (PdfCompat.PdfA3B, "PDF/A-3B", "pdfa3b_test.pdf"),
            (PdfCompat.PdfA3U, "PDF/A-3U", "pdfa3u_test.pdf")
        };

        var tasks = testCases.Select(testCase =>
        {
            using var image = CreateScannedImage();
            var path = Path.Combine(FolderPath, testCase.fileName);
            pdfExporter.Export(path, new[] { image }, new PdfExportParams
            {
                Compat = testCase.pdfCompat
            }).Wait();
            return PdfAsserts.AssertCompliant(testCase.profile, path);
        }).ToArray();
        await Task.WhenAll(tasks);
    }
}