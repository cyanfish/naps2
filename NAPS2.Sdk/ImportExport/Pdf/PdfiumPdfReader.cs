using System.Text;

namespace NAPS2.ImportExport.Pdf;

public class PdfiumPdfReader
{
    public PdfiumPdfReader()
    {
    }

    public IEnumerable<string> ReadTextByPage(string path)
    {
        var nativeLib = PdfiumNativeLibrary.LazyInstance.Value;

        // Pdfium is not thread-safe
        lock (nativeLib)
        {
            var doc = nativeLib.FPDF_LoadDocument(path, null);
            try
            {
                var pageCount = nativeLib.FPDF_GetPageCount(doc);
                for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
                {
                    var page = nativeLib.FPDF_LoadPage(doc, pageIndex);
                    try
                    {
                        var textPage = nativeLib.FPDFText_LoadPage(page);
                        try
                        {
                            var charCount = nativeLib.FPDFText_CountChars(textPage);
                            var sb = new StringBuilder();
                            for (int i = 0; i < charCount; i++)
                            {
                                var unicode = nativeLib.FPDFText_GetUnicode(textPage, i);
                                string str = char.ConvertFromUtf32(unicode);
                                // Exclude control characters apart from whitespace
                                if (str.Length != 1 || !char.IsControl(str[0]) || char.IsWhiteSpace(str[0]))
                                {
                                    sb.Append(str);                                    
                                }
                            }
                            yield return sb.ToString();
                        }
                        finally
                        {
                            nativeLib.FPDFText_ClosePage(textPage);
                        }
                    }
                    finally
                    {
                        nativeLib.FPDF_ClosePage(page);
                    }
                }
            }
            finally
            {
                nativeLib.FPDF_CloseDocument(doc);
            }
        }
    }
}