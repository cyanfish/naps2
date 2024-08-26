using Eto.Drawing;

namespace NAPS2.EtoForms;

public class DefaultIconProvider : IIconProvider
{
    public Bitmap? GetIcon(string name, bool oversized = false)
    {
        var data = (byte[]?) Icons.ResourceManager.GetObject(name);
        if (data == null) return null;
        return new Bitmap(data);
    }

    public Icon? GetFormIcon(string name)
    {
        var icon = GetIcon(name);
        return icon != null ? new Icon(1f, icon) : null;
    }
}