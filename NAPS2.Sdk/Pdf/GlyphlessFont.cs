using System.Text;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Advanced;

namespace NAPS2.Pdf;

/// <summary>
/// A font with a single glyph for writing invisible OCR text in exported PDFs. The font program (GlyphlessFont.ttf) is
/// Tesseract's "GlyphLessFont" from https://github.com/tesseract-ocr/tesseract/blob/main/tessdata/pdf.ttf (Apache-2.0).
/// </summary>
internal static class GlyphlessFont
{
    private const string FontName = "GlyphLessFont";
    public const string ResourceName = "/FGlyphless";

    /// <summary>
    /// The advance width of every glyph in 1/1000 em.
    /// </summary>
    public const int GlyphWidth = 500;

    /// <summary>
    /// Adds the font objects to the document and returns a reference to the Type0 font dictionary.
    /// </summary>
    public static PdfReference AddToDocument(PdfDocument document)
    {
        var fontProgram = LoadFontProgram();
        var fontFile = AddStream(document, fontProgram, new PdfDictionary(document)
        {
            Elements = { ["/Length1"] = new PdfInteger(fontProgram.Length) }
        });

        var descriptor = AddObject(document, new PdfDictionary(document)
        {
            Elements =
            {
                ["/Type"] = new PdfName("/FontDescriptor"),
                ["/FontName"] = new PdfName("/" + FontName),
                // FixedPitch + Symbolic
                ["/Flags"] = new PdfInteger(5),
                // These metrics (in 1/1000 em) need to match the font program
                ["/FontBBox"] = new PdfArray(document,
                    new PdfInteger(0), new PdfInteger(0), new PdfInteger(GlyphWidth), new PdfInteger(1000)),
                ["/ItalicAngle"] = new PdfInteger(0),
                ["/Ascent"] = new PdfInteger(1000),
                ["/Descent"] = new PdfInteger(-1),
                ["/CapHeight"] = new PdfInteger(1000),
                ["/StemV"] = new PdfInteger(80),
                ["/FontFile2"] = fontFile
            }
        });

        var cidToGidMap = AddStream(document, CreateCidToGidMap());

        var cidFont = AddObject(document, new PdfDictionary(document)
        {
            Elements =
            {
                ["/Type"] = new PdfName("/Font"),
                ["/Subtype"] = new PdfName("/CIDFontType2"),
                ["/BaseFont"] = new PdfName("/" + FontName),
                ["/CIDSystemInfo"] = new PdfDictionary(document)
                {
                    Elements =
                    {
                        ["/Registry"] = new PdfString("Adobe"),
                        ["/Ordering"] = new PdfString("Identity"),
                        ["/Supplement"] = new PdfInteger(0)
                    }
                },
                ["/FontDescriptor"] = descriptor,
                ["/DW"] = new PdfInteger(GlyphWidth),
                ["/CIDToGIDMap"] = cidToGidMap
            }
        });

        var toUnicode = AddStream(document, Encoding.ASCII.GetBytes(BuildToUnicodeCMap()));

        return AddObject(document, new PdfDictionary(document)
        {
            Elements =
            {
                ["/Type"] = new PdfName("/Font"),
                ["/Subtype"] = new PdfName("/Type0"),
                ["/BaseFont"] = new PdfName("/" + FontName),
                ["/Encoding"] = new PdfName("/Identity-H"),
                ["/DescendantFonts"] = new PdfArray(document, cidFont),
                ["/ToUnicode"] = toUnicode
            }
        });
    }

    /// <summary>
    /// Loads the font into a Pdfium document. Pdfium generates the font dictionaries itself (with equivalent values),
    /// using the CIDToGIDMap and ToUnicode CMap we provide.
    /// </summary>
    public static Pdfium.PdfFont AddToPdfiumDocument(Pdfium.PdfDocument document)
    {
        return document.LoadCidType2Font(LoadFontProgram(), BuildToUnicodeCMap(), CreateCidToGidMap());
    }

    /// <summary>
    /// Encodes the text as a PDF hex string for use with the Tj operator. Each UTF-16 code unit is one character code.
    /// </summary>
    public static string EncodeText(string text) =>
        "<" + string.Concat(text.Select(c => ((int) c).ToString("X4"))) + ">";

    private static byte[] CreateCidToGidMap()
    {
        // Map every CID to glyph 1 (the rectangle). We avoid glyph 0 as PDF/A disallows referencing .notdef.
        var map = new byte[65536 * 2];
        for (int i = 1; i < map.Length; i += 2)
        {
            map[i] = 1;
        }
        return map;
    }

    private static string BuildToUnicodeCMap()
    {
        // Map each code to the same UTF-16 code unit. A bfrange can't cross a boundary in its first byte and can have
        // at most 100 entries per block, so we split the full range into 256 ranges.
        var bfranges = string.Join("\n", Enumerable.Range(0, 256)
            .GroupBy(hi => hi / 100)
            .Select(block => $"""
                {block.Count()} beginbfrange
                {string.Join("\n", block.Select(hi => $"<{hi:X2}00> <{hi:X2}FF> <{hi:X2}00>"))}
                endbfrange
                """));
        var cmap = $"""
            /CIDInit /ProcSet findresource begin
            12 dict begin
            begincmap
            /CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def
            /CMapName /Adobe-Identity-UCS def
            /CMapType 2 def
            1 begincodespacerange
            <0000> <FFFF>
            endcodespacerange
            {bfranges}
            endcmap
            CMapName currentdict /CMap defineresource pop
            end
            end
            """;
        // Raw string literals use the source file's line endings, so normalize them for consistent output
        return cmap.Replace("\r\n", "\n");
    }

    private static PdfReference AddObject(PdfDocument document, PdfDictionary dict)
    {
        document.Internals.AddObject(dict);
        return dict.Reference;
    }

    private static PdfReference AddStream(PdfDocument document, byte[] data, PdfDictionary? dict = null)
    {
        dict ??= new PdfDictionary(document);
        dict.CreateStream(data);
        dict.Stream.Zip();
        return AddObject(document, dict);
    }

    private static byte[] LoadFontProgram()
    {
        using var stream = typeof(GlyphlessFont).Assembly.GetManifestResourceStream("NAPS2.Pdf.GlyphlessFont.ttf")
                           ?? throw new InvalidOperationException("Missing GlyphlessFont.ttf resource");
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
