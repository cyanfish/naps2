using System.Text;

namespace NAPS2.Pdf.Pdfium;

internal class PdfText : NativePdfiumObject
{
    internal PdfText(IntPtr handle) : base(handle)
    {
    }

    public int CharCount => Native.FPDFText_CountChars(Handle);

    public int ReadUtf32(int i) => Native.FPDFText_GetUnicode(Handle, i);

    public PdfRect GetCharBox(int i)
    {
        if (!Native.FPDFText_GetCharBox(Handle, i, out var left, out var right, out var bottom, out var top))
        {
            throw new Exception("Could not get char box");
        }
        return new PdfRect((float) left, (float) top, (float) right, (float) bottom);
    }

    public string ReadAll()
    {
        var charCount = CharCount;
        var sb = new StringBuilder();
        for (int i = 0; i < charCount; i++)
        {
            int value = ReadUtf32(i);
            // Characters outside the BMP may be returned as separate UTF-16 surrogates
            string str = value is >= 0xD800 and <= 0xDFFF ? ((char) value).ToString() : char.ConvertFromUtf32(value);
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