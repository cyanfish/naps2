using System.Text;
using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfReader
{
    public IEnumerable<string> ReadTextByPage(string path)
    {
        // Pdfium is not thread-safe
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path);
            var pageCount = doc.PageCount;
            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                using var page = doc.GetPage(pageIndex);
                using var text = page.GetText();
                yield return text.ReadAll();
            }
        }
    }
}