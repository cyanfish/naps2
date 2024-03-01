using Microsoft.Extensions.Logging;
using NAPS2.Ocr;
using NAPS2.Pdf;
using NAPS2.Sdk.Tests.Asserts;
using PdfSharpCore.Utils;
using Xunit;
using Xunit.Abstractions;
using Alphabet = NAPS2.Pdf.PdfFontPicker.Alphabet;

namespace NAPS2.Sdk.Tests.Pdf;

// As we use the same data for multiple methods, some parameters may be unused
#pragma warning disable xUnit1026

public class PdfFontTests : ContextualTests
{
    private readonly PdfImporter _importer;
    private readonly PdfExporter _exporter;
    private readonly string _exportPath;
    private readonly string _pdfiumImportPath;

    public PdfFontTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _importer = new PdfImporter(ScanningContext);
        _exporter = new PdfExporter(ScanningContext);
        _exportPath = Path.Combine(FolderPath, "test.pdf");
        _pdfiumImportPath = Path.Combine(FolderPath, "import_ocr.pdf");
        File.WriteAllBytes(_pdfiumImportPath, PdfResources.word_ocr_test);
    }

    [Fact]
    public void CheckAvailableFonts()
    {
        foreach (var font in FontResolver.InstalledFonts.OrderBy(x => x.Key))
        {
            ScanningContext.Logger.LogDebug($"Font: {font.Key}");
        }
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal void MapLanguageCodeToAlphabet(Alphabet alphabet, string langCode, string text, bool rtl)
    {
        Assert.Equal(alphabet, PdfFontPicker.MapLanguageCodeToAlphabet(langCode));
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal async Task ExportAlphabetsWithPdfSharp(Alphabet alphabet, string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(ifNoMatch: text, delay: 0);

        using var image = CreateScannedImage();
        await _exporter.Export(_exportPath, [image], ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
        // Rough verification that a font subset is used instead of embedding the whole font
        Assert.InRange(new FileInfo(_exportPath).Length, 1, 500_000);
    }

    [Theory]
    [MemberData(nameof(AlphabetTestCases))]
    internal async Task ExportAlphabetsWithPdfium(Alphabet alphabet, string langCode, string text, bool rtl)
    {
        SetUpFakeOcr(ifNoMatch: text, delay: 0);

        var images = await _importer.Import(_pdfiumImportPath).ToListAsync();
        await _exporter.Export(_exportPath, images, ocrParams: new OcrParams(langCode));

        if (rtl)
        {
            text = new string(text.Reverse().ToArray());
        }
        PdfAsserts.AssertContainsTextOnce(text, _exportPath);
        // Rough verification that a font subset is used instead of embedding the whole font
        // TODO: It seems like Pdfium fonts are bigger than PdfSharp - maybe not compressed? Can we improve that?
        Assert.InRange(new FileInfo(_exportPath).Length, 1, 700_000);
    }

    public static IEnumerable<object[]> AlphabetTestCases =
    [
        new object[] { Alphabet.Latin, "eng", "Hello world", false },
        new object[] { Alphabet.Cyrillic, "rus", "Привет, мир", false },
        new object[] { Alphabet.Greek, "ell", "Γειά σου Κόσμε", false },
        new object[] { Alphabet.Hebrew, "heb", "שלום עולם", true },
        new object[] { Alphabet.Arabic, "ara", "مرحبا بالعالم", true },
        new object[] { Alphabet.Armenian, "hye", "Բարեւ աշխարհ", false },
        new object[] { Alphabet.Bengali, "ben", "ওহে বিশ্ব", false },
        new object[] { Alphabet.CanadianAboriginal, "iku", "ᐃᓄᒃᑎᑐᑦ", false },
        new object[] { Alphabet.Cherokee, "chr", "ᏣᎳᎩ ᎦᏬᏂᎯᏍᏗ", false },
        new object[] { Alphabet.Devanagari, "hin", "ह\u0948ल\u094b वर\u094dल\u094dड", false },
        new object[] { Alphabet.Ethiopic, "amh", "ሰላም ልዑል", false },
        new object[] { Alphabet.Georgian, "kat", "Გამარჯობა მსოფლიო", false },
        new object[] { Alphabet.Gujarati, "guj", "હ\u0ac7લ\u0acb વર\u0acdલ\u0acdડ", false },
        new object[] { Alphabet.Gurmukhi, "pan", "ਸਤ\u0a3f ਸ\u0a4dਰ\u0a40 ਅਕ\u0a3eਲ ਦ\u0a41ਨ\u0a3fਆ", false },
        new object[] { Alphabet.Kannada, "kan", "ಹಲ\u0ccbವರ\u0ccdಲ\u0ccdಡ\u0ccd", false },
        new object[] { Alphabet.Khmer, "khm", "ស\u17bdស\u17d2ត\u17b8\u200bព\u17b7ភពល\u17c4ក", false },
        new object[] { Alphabet.Lao, "lao", "ສະ\u200bບາຍ\u200bດ\u0eb5\u200bຊາວ\u200bໂລກ", false },
        new object[] { Alphabet.Malayalam, "mal", "ഹല\u0d47\u0d3e വ\u0d47ൾഡ\u0d4d", false },
        new object[] { Alphabet.Myanmar, "mya", "မင\u103a\u1039ဂလ\u102cပ\u102bကမ\u1039ဘ\u102cလ\u1031\u102cက", false },
        new object[] { Alphabet.Sinhala, "sin", "හ\u0dd9ල\u0dddවර\u0dcaල\u0dcaඩ\u0dca", false },
        // Not running by default as it requires a supplemental font on Windows
        // new object[] { Alphabet.Syriac, "syr", "ܐܘ ܢ\u0733ܫܐ ܟ\u0737ܬܠ\u0736ܗ", true },
        new object[] { Alphabet.Tamil, "tam", "வணக\u0bcdகம\u0bcdஉலகம\u0bcd", false },
        new object[] { Alphabet.Telugu, "tel", "హల\u0c4bవరల\u0c4dడ\u0c4d", false },
        new object[] { Alphabet.Thaana, "div", "ހ\u07acލ\u07afދ\u07aaނ\u07a8ޔ\u07ac", true },
        new object[] { Alphabet.Thai, "tha", "สว\u0e31สด\u0e35ชาวโลก", false },
        new object[] { Alphabet.Tibetan, "bod", "བ\u0f7cད་ས\u0f90ད་", false },
        new object[] { Alphabet.ChineseSimplified, "chi_sim", "你好复杂的世界", false },
        new object[] { Alphabet.ChineseTraditional, "chi_tra", "你好複雜的世界", false },
        new object[] { Alphabet.Japanese, "jpn", "こんにちは世界", false },
        new object[] { Alphabet.Korean, "kor", "안녕하세요 세상", false },
    ];
}