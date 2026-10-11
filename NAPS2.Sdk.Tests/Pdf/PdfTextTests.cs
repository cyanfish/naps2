using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using Xunit;
using Xunit.Abstractions;

namespace NAPS2.Sdk.Tests.Pdf;

// As we use the same data for multiple methods, some parameters may be unused
#pragma warning disable xUnit1026

public class PdfTextTests : ContextualTests
{
    private readonly PdfImporter _importer;
    private readonly PdfExporter _exporter;
    private readonly string _exportPath;
    private readonly string _pdfiumImportPath;

    public PdfTextTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _importer = new PdfImporter(ScanningContext);
        _exporter = new PdfExporter(ScanningContext);
        _exportPath = Path.Combine(FolderPath, "test.pdf");
        _pdfiumImportPath = Path.Combine(FolderPath, "import_ocr.pdf");
        File.WriteAllBytes(_pdfiumImportPath, PdfResources.word_ocr_test);
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal void OcrAlphabetFromLanguageCode(OcrAlphabet alphabet, string langCode, string text, bool rtl)
    {
        Assert.Equal(alphabet, OcrAlphabet.FromLanguageCode(langCode));
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal async Task ExportAlphabetsWithPdfSharp(OcrAlphabet alphabet, string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(ifNoMatch: text, delay: 0);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
        await AssertSmallOcrOverhead([image]);
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal async Task ExportAlphabetsWithPdfium(OcrAlphabet alphabet, string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(ifNoMatch: text, delay: 0);

        var images = await _importer.Import(_pdfiumImportPath).ToListAsync();
        await _exporter.Export(_exportPath, images, ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
        await AssertSmallOcrOverhead(images);
    }

    [Fact]
    public async Task ExportSeparateWordsWithPdfSharp()
    {
        SetUpFakeOcr(ifNoMatch: "Hello world", delay: 0, splitWords: true);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams("eng"));

        PdfAsserts.AssertContainsTextOnce("Hello world", _exportPath);
    }

    [Fact]
    public async Task ExportCjkWordsWithPdfSharp()
    {
        SetUpFakeOcr(ifNoMatch: "你 好 世 界", delay: 0, splitWords: true);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams("chi_sim"));

        // No spaces should be added between CJK words
        PdfAsserts.AssertContainsTextOnce("你好世界", _exportPath);
    }

    [Fact]
    public async Task ExportNonBmpWithPdfSharp()
    {
        // CJK Unified Ideographs Extension B, which need surrogate pairs in UTF-16
        var text = "\U00020000\U00020001";
        SetUpFakeOcr(ifNoMatch: text, delay: 0);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams("chi_sim"));

        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
    }

    [Fact]
    public async Task ExportMultipleLinesWithPdfSharp()
    {
        var text = "Hello world\nfoo bar baz";
        SetUpFakeOcr(ifNoMatch: text, delay: 0, splitWords: true);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams("eng"));

        PdfAsserts.AssertContainsTextOnce("Hello world foo bar baz", _exportPath);
        PdfAsserts.AssertOcrText(CreateFakeOcrResult(text, "eng", true), _exportPath);
    }

    [Fact]
    public async Task ExportMultipleLinesWithPdfium()
    {
        var text = "Hello world\nfoo bar baz";
        SetUpFakeOcr(ifNoMatch: text, delay: 0, splitWords: true);

        var images = await _importer.Import(_pdfiumImportPath).ToListAsync();
        await _exporter.Export(_exportPath, images, ocrParams: new OcrParams("eng"));

        PdfAsserts.AssertContainsTextOnce("Hello world foo bar baz", _exportPath);
        PdfAsserts.AssertOcrText(CreateFakeOcrResult(text, "eng", true), _exportPath);
    }

    private async Task AssertSmallOcrOverhead(ICollection<ProcessedImage> images)
    {
        // The glyphless font is tiny and doesn't depend on the language, so OCR text should only add a few KB (~3.5KB)
        // compared to exporting without OCR
        var sizeWithOcr = new FileInfo(_exportPath).Length;
        await _exporter.Export(_exportPath, images);
        var sizeWithoutOcr = new FileInfo(_exportPath).Length;
        Assert.InRange(sizeWithOcr - sizeWithoutOcr, 1, 10_000);
    }

    public static IEnumerable<object[]> AlphabetTestCases =
    [
        new object[] { OcrAlphabet.Latin, "eng", "Hello world", false },
        new object[] { OcrAlphabet.Cyrillic, "rus", "Привет, мир", false },
        new object[] { OcrAlphabet.Greek, "ell", "Γειά σου Κόσμε", false },
        new object[] { OcrAlphabet.Hebrew, "heb", "שלום עולם", true },
        new object[] { OcrAlphabet.Arabic, "ara", "مرحبا بالعالم", true },
        new object[] { OcrAlphabet.Armenian, "hye", "Բարեւ աշխարհ", false },
        new object[] { OcrAlphabet.Bengali, "ben", "ওহে বিশ্ব", false },
        new object[] { OcrAlphabet.CanadianAboriginal, "iku", "ᐃᓄᒃᑎᑐᑦ", false },
        new object[] { OcrAlphabet.Cherokee, "chr", "ᏣᎳᎩ ᎦᏬᏂᎯᏍᏗ", false },
        new object[] { OcrAlphabet.Devanagari, "hin", "ह\u0948ल\u094b वर\u094dल\u094dड", false },
        new object[] { OcrAlphabet.Ethiopic, "amh", "ሰላም ልዑል", false },
        new object[] { OcrAlphabet.Georgian, "kat", "Გამარჯობა მსოფლიო", false },
        new object[] { OcrAlphabet.Gujarati, "guj", "હ\u0ac7લ\u0acb વર\u0acdલ\u0acdડ", false },
        new object[] { OcrAlphabet.Gurmukhi, "pan", "ਸਤ\u0a3f ਸ\u0a4dਰ\u0a40 ਅਕ\u0a3eਲ ਦ\u0a41ਨ\u0a3fਆ", false },
        new object[] { OcrAlphabet.Kannada, "kan", "ಹಲ\u0ccbವರ\u0ccdಲ\u0ccdಡ\u0ccd", false },
        new object[] { OcrAlphabet.Khmer, "khm", "ស\u17bdស\u17d2ត\u17b8\u200bព\u17b7ភពល\u17c4ក", false },
        new object[] { OcrAlphabet.Lao, "lao", "ສະ\u200bບາຍ\u200bດ\u0eb5\u200bຊາວ\u200bໂລກ", false },
        new object[] { OcrAlphabet.Malayalam, "mal", "ഹല\u0d47\u0d3e വ\u0d47ൾഡ\u0d4d", false },
        new object[] { OcrAlphabet.Myanmar, "mya", "မင\u103a\u1039ဂလ\u102cပ\u102bကမ\u1039ဘ\u102cလ\u1031\u102cက", false },
        new object[] { OcrAlphabet.Sinhala, "sin", "හ\u0dd9ල\u0dddවර\u0dcaල\u0dcaඩ\u0dca", false },
        new object[] { OcrAlphabet.Syriac, "syr", "ܐܘ ܢ\u0733ܫܐ ܟ\u0737ܬܠ\u0736ܗ", true },
        new object[] { OcrAlphabet.Tamil, "tam", "வணக\u0bcdகம\u0bcdஉலகம\u0bcd", false },
        new object[] { OcrAlphabet.Telugu, "tel", "హల\u0c4bవరల\u0c4dడ\u0c4d", false },
        new object[] { OcrAlphabet.Thaana, "div", "ހ\u07acލ\u07afދ\u07aaނ\u07a8ޔ\u07ac", true },
        new object[] { OcrAlphabet.Thai, "tha", "สว\u0e31สด\u0e35ชาวโลก", false },
        new object[] { OcrAlphabet.Tibetan, "bod", "བ\u0f7cད་ས\u0f90ད་", false },
        new object[] { OcrAlphabet.ChineseSimplified, "chi_sim", "你好复杂的世界", false },
        new object[] { OcrAlphabet.ChineseTraditional, "chi_tra", "你好複雜的世界", false },
        new object[] { OcrAlphabet.Japanese, "jpn", "こんにちは世界", false },
        new object[] { OcrAlphabet.Korean, "kor", "안녕하세요 세상", false },
    ];
}