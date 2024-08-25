using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IIconProvider
{
    Bitmap? GetIcon(string name, bool oversized = false);
}