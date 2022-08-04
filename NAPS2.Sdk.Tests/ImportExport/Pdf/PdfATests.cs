using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport.Pdf;

// TODO: Validate with OCR output
// TODO: Maaaybe validate with external import? We certainly can't guarantee it, but maybe some cases can be verified for best effort
public class PdfATests : ContextualTests
{
    [Fact]
    public void Validate()
    {
        var pdfExporter = new PdfExporter(ScanningContext);
        var testCases = new (PdfCompat pdfCompat, string profile, string fileName)[]
        {
            (PdfCompat.PdfA1B, "PDF/A-1B", "pdfa1b_test.pdf"),
            (PdfCompat.PdfA2B, "PDF/A-2B", "pdfa2b_test.pdf"),
            (PdfCompat.PdfA3B, "PDF/A-3B", "pdfa3b_test.pdf"),
            (PdfCompat.PdfA3U, "PDF/A-3U", "pdfa3u_test.pdf")
        };

        Parallel.ForEach(testCases, testCase =>
        {
            using var image = CreateScannedImage();
            var path = Path.Combine(FolderPath, testCase.fileName);
            pdfExporter.Export(path, new[] { image }, new PdfExportParams
            {
                Compat = testCase.pdfCompat
            }).Wait();
            PdfAsserts.AssertCompliant(testCase.profile, path);
        });
    }
}