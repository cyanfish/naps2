using NAPS2.Ocr;
using NAPS2.Pdf.Pdfium;
using PdfSharpCore.Utils;

namespace NAPS2.Pdf;

/// <summary>
/// Creates and manages the lifetime of font subsets for Pdfium exporting.
/// </summary>
internal class PdfiumFontSubsets : IDisposable
{
    private readonly Dictionary<string, PdfFont> _fonts;

    public PdfiumFontSubsets(PdfDocument pdfiumDocument, IEnumerable<OcrResult?> ocrResults)
    {
        var fontSubsetBuilders = new Dictionary<string, FontSubsetBuilder>();
        foreach (var element in ocrResults.WhereNotNull().SelectMany(result => result.Words))
        {
            // Map the OCR language to a font that supports its glyphs
            var fontName = PdfFontPicker.GetBestFont(element.LanguageCode);
            // TODO: What happens if the font name isn't found?
            var builder = fontSubsetBuilders.GetOrSet(fontName, () => new FontSubsetBuilder(fontName));
            // Include the glyphs from the current text in the font subset
            builder.AddGlyphs(element.Text);
        }
        // Load each font subset into Pdfium
        _fonts = fontSubsetBuilders.ToDictionary(kvp => kvp.Key, kvp => pdfiumDocument.LoadFont(kvp.Value.Build()));
    }

    public PdfFont this[string fontName] => _fonts[fontName];

    public void Dispose()
    {
        foreach (var font in _fonts.Values)
        {
            font.Dispose();
        }
    }
}