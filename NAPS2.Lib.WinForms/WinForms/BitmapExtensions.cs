using System.Drawing;

namespace NAPS2.WinForms;

public static class BitmapExtensions
{
    public static Bitmap ToBitmap(this byte[] bytes) => new(new MemoryStream(bytes));
}