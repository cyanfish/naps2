using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Pdf;

public class PdfATests : ContextualTests
{
    private readonly PdfExporter _pdfExporter;
    private readonly string _path;
    private readonly string _importPath;

    public PdfATests()
    {
        _pdfExporter = new PdfExporter(ScanningContext);
        _path = Path.Combine(FolderPath, "test.pdf");
        _importPath = CopyResourceToFile(PdfResources.word_patcht_pdf, "word.pdf");
    }

    // Sadly the pdfa verifier library only supports windows/linux
    [PlatformTheory(exclude: PlatformFlags.Mac)]
    [MemberData(nameof(TestCases))]
    public async Task Validate(PdfCompat pdfCompat, string profile, int version)
    {
        await _pdfExporter.Export(_path, new[] { CreateScannedImage() }, new PdfExportParams
        {
            Compat = pdfCompat
        });

        PdfAsserts.AssertVersion(version, _path);
        await PdfAsserts.AssertCompliant(profile, _path);
    }

    [PlatformTheory(exclude: PlatformFlags.Mac)]
    [MemberData(nameof(TestCases))]
    public async Task ValidateWithOcr(PdfCompat pdfCompat, string profile, int version)
    {
        SetUpFakeOcr(ifNoMatch: "hello world");

        await _pdfExporter.Export(_path, new[] { CreateScannedImage() }, new PdfExportParams
        {
            Compat = pdfCompat
        }, new OcrParams("eng"));

        PdfAsserts.AssertVersion(version, _path);
        await PdfAsserts.AssertCompliant(profile, _path);
    }

    [PlatformTheory(exclude: PlatformFlags.Mac)]
    [MemberData(nameof(TestCases))]
    public async Task ValidateWithPdfium(PdfCompat pdfCompat, string profile, int version)
    {
        var images = await new PdfImporter(ScanningContext).Import(_importPath).ToListAsync();

        await _pdfExporter.Export(_path, images, new PdfExportParams
        {
            Compat = pdfCompat
        });

        PdfAsserts.AssertVersion(version, _path);
        await PdfAsserts.AssertCompliant(profile, _path);
    }

    // Note that we don't have a Pdfium OCR test as we fail compliance due to the way Pdfium embeds fonts, which isn't
    // practical to fix.

    public static IEnumerable<object[]> TestCases =
    [
        [PdfCompat.Default, "", 14],
        [PdfCompat.PdfA1B, "PDF/A-1B", 14],
        [PdfCompat.PdfA2B, "PDF/A-2B", 17],
        [PdfCompat.PdfA3B, "PDF/A-3B", 17],
        [PdfCompat.PdfA3U, "PDF/A-3U", 17]
    ];
}