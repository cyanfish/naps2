using Eto.Drawing;

namespace NAPS2.EtoForms;

public interface IIconProvider
{
    Bitmap? GetIcon(string name, IconScenario scenario, float scale = 1f);

    Icon? ToFormIcon(Bitmap bitmap);
}