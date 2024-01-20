using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;

namespace NAPS2.Sdk.Tests.Pdf;

public class PdfFontTests : ContextualTests
{
    private readonly PdfImporter _importer;
    private readonly PdfExporter _exporter;
    private readonly string _exportPath;
    private readonly string _pdfiumImportPath;

    public PdfFontTests()
    {
        _importer = new PdfImporter(ScanningContext);
        _exporter = new PdfExporter(ScanningContext);
        _exportPath = Path.Combine(FolderPath, "test.pdf");
        _pdfiumImportPath = Path.Combine(FolderPath, "import_ocr.pdf");
        File.WriteAllBytes(_pdfiumImportPath, PdfResources.word_patcht_pdf);
    }

    // TODO: Maybe make these not run on Linux by default if it requires a specific package like noto-fonts-cjk
    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    public async Task ExportAlphabetsWithPdfSharp(string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(new()
        {
            { LoadImage(ImageResources.dog), text }
        });

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    public async Task ExportAlphabetsWithPdfium(string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(new()
        {
            { LoadImage(PdfResources.word_patcht_p1), text }
        });

        var images = await _importer.Import(_pdfiumImportPath).ToListAsync();
        await _exporter.Export(_exportPath, images, ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
    }

    public static IEnumerable<object[]> AlphabetTestCases =
    [
        new object[] { "eng", "Hello world", false },
        new object[] { "heb", "שלום עולם", true },
        new object[] { "ell", "Γειά σου Κόσμε", false },
        new object[] { "kor", "안녕하세요 세상", false },
        new object[] { "zhs", "你好世界", false },
        new object[] { "jpn", "こんにちは世界", false },
        new object[] { "ara", "مرحبا بالعالم", true }
    ];
}