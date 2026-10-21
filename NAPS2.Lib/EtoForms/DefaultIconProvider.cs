using Eto.Drawing;

namespace NAPS2.EtoForms;

public class DefaultIconProvider : IIconProvider
{
    public Bitmap? GetIcon(string name, IconScenario scenario, float scale = 1f)
    {
        int size = scenario.ToSize();
        // Prefer downscaling a double-size icon over upscaling (or when there's no icon of the requested size)
        var hires = GetIconData(name, size * 2);
        var data = GetIconData(name, size);
        if (hires != null && (scale > 1 || data == null))
        {
            return new Bitmap(hires).ResizeTo((int) (size * scale));
        }
        if (data != null)
        {
            var bitmap = new Bitmap(data);
            if (scale > 1)
            {
                return bitmap.ResizeTo((int) (bitmap.Width * scale), (int) (bitmap.Height * scale));
            }
            return bitmap;
        }
        return null;
    }

    private static byte[]? GetIconData(string name, int size) =>
        (byte[]?) Icons.ResourceManager.GetObject($"{name}_{size}");

    public Icon ToFormIcon(Bitmap bitmap) => new(1f, bitmap);
}