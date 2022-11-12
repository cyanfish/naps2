using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IIconProvider
{
    Image? GetIcon(string name);
}