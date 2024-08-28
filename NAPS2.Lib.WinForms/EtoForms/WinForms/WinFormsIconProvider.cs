using Eto.Drawing;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsIconProvider : IIconProvider
{
    public Bitmap? GetIcon(string name, float scale = 1f, bool oversized = false)
    {
        if (scale > 1)
        {
            if (name.EndsWith("_small"))
            {
                var norm = (byte[]?) Icons.ResourceManager.GetObject(name.Substring(0, name.Length - 6));
                if (norm != null)
                {
                    return new Bitmap(norm).ResizeTo((int) (16 * scale));
                }
            }
            else
            {
                var hires = (byte[]?) Icons.ResourceManager.GetObject(name + "_hires");
                if (hires != null)
                {
                    return new Bitmap(hires).ResizeTo((int) (32 * scale));
                }
            }
        }

        var data = (byte[]?) Icons.ResourceManager.GetObject(name);
        if (data != null)
        {
            return new Bitmap(data);
        }

        return null;
    }

    public Icon? GetFormIcon(string name, float scale = 1f)
    {
        var icon = GetIcon(name, scale);
        return icon != null ? new Icon(1f, icon) : null;
    }
}