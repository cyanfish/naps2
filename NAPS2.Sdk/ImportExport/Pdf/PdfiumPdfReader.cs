using System.Runtime.InteropServices;
using NAPS2.ImportExport.Pdf.Pdfium;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfReader
{
    public PdfMetadata ReadMetadata(string path, string? password = null)
    {
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path, password);
            return new PdfMetadata
            {
                Author = doc.GetMetaText("Author"),
                Title = doc.GetMetaText("Title"),
                Subject = doc.GetMetaText("Subject"),
                Keywords = doc.GetMetaText("Keywords"),
                Creator = doc.GetMetaText("Creator")
            };
        }
    }

    public IEnumerable<string> ReadTextByPage(string path, string? password = null)
    {
        lock (PdfiumNativeLibrary.Instance)
        {
            using var doc = PdfDocument.Load(path, password);
            foreach (var text in DoReadTextByPage(doc))
            {
                yield return text;
            }
        }
    }

    public IEnumerable<string> ReadTextByPage(byte[] buffer, int length, string? password = null)
    {
        lock (PdfiumNativeLibrary.Instance)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                using var doc = PdfDocument.Load(handle.AddrOfPinnedObject(), length, password);
                foreach (var text in DoReadTextByPage(doc))
                {
                    yield return text;
                }
            }
            finally
            {
                handle.Free();
            }
        }
    }

    private static IEnumerable<string> DoReadTextByPage(PdfDocument doc)
    {
        var pageCount = doc.PageCount;
        for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            using var page = doc.GetPage(pageIndex);
            using var text = page.GetText();
            yield return text.ReadAll();
        }
    }
}