using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Desktop;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsIconProvider : IIconProvider
{
    private const int DEFAULT_DPI = 96;

    private readonly DesktopFormProvider _desktopFormProvider;

    public WinFormsIconProvider(DesktopFormProvider desktopFormProvider)
    {
        _desktopFormProvider = desktopFormProvider;
    }

    public Bitmap? GetIcon(string name, bool oversized = false)
    {
        var dpi = _desktopFormProvider.DesktopForm.ToNative().DeviceDpi;

        if (dpi > DEFAULT_DPI)
        {
            if (name.EndsWith("_small"))
            {
                var norm = (byte[]?) Icons.ResourceManager.GetObject(name.Substring(0, name.Length - 6));
                if (norm != null)
                {
                    return new Bitmap(norm).ResizeTo(16 * dpi / DEFAULT_DPI);
                }
            }
            else
            {
                var hires = (byte[]?) Icons.ResourceManager.GetObject(name + "_hires");
                if (hires != null)
                {
                    return new Bitmap(hires).ResizeTo(32 * dpi / DEFAULT_DPI);
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

    public Icon? GetFormIcon(string name)
    {
        var icon = GetIcon(name);
        return icon != null ? new Icon(1f, icon) : null;
    }
}