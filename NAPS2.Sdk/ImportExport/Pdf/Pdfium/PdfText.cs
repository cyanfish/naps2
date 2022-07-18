using System.Text;

namespace NAPS2.ImportExport.Pdf.Pdfium;

public class PdfText : NativePdfiumObject
{
    internal PdfText(IntPtr handle) : base(handle)
    {
    }

    public int CharCount => Native.FPDFText_CountChars(Handle);

    public int ReadUtf32(int i) => Native.FPDFText_GetUnicode(Handle, i);

    public string ReadAll()
    {
        var charCount = CharCount;
        var sb = new StringBuilder();
        for (int i = 0; i < charCount; i++)
        {
            string str = char.ConvertFromUtf32(ReadUtf32(i));
            // Exclude control characters apart from whitespace
            if (str.Length != 1 || !char.IsControl(str[0]) || char.IsWhiteSpace(str[0]))
            {
                sb.Append(str);
            }
        }
        return sb.ToString();
    }

    protected override void DisposeHandle()
    {
        Native.FPDFText_ClosePage(Handle);
    }
}