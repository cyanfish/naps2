using Eto.Drawing;

namespace NAPS2.EtoForms;

public class DefaultIconProvider : IIconProvider
{
    public Image GetIcon(string name)
    {
        return new Bitmap((byte[]) (Icons.ResourceManager.GetObject(name) ?? throw new ArgumentException()));
    }
}