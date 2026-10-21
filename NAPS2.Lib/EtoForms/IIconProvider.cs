using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IIconProvider
{
    Bitmap? GetIcon(string name, int size, float scale = 1f, bool oversized = false);

    Icon? ToFormIcon(Bitmap bitmap);
}