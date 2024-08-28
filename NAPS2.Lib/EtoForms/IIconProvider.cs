using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IIconProvider
{
    Bitmap? GetIcon(string name, float scale = 1f, bool oversized = false);

    Icon? GetFormIcon(string name, float scale = 1f);
}