using NAPS2.Images.Gdi;
using NAPS2.ImportExport.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using NAPS2.Sdk.Tests.Images;
using Xunit;

namespace NAPS2.Sdk.Tests.ImportExport;

public class PdfATests : ContextualTexts
{
    [Fact]
    public void Validate()
    {
        var pdfExporter = new PdfSharpExporter(ScanningContext);
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
            pdfExporter.Export(testCase.fileName, new[] { image }, new PdfSettings
            {
                Compat = testCase.pdfCompat
            }).Wait();
            PdfAsserts.AssertCompliant(testCase.profile, testCase.fileName);
        });
    }
}