using Eto.Drawing;
using Eto.Mac.Drawing;

namespace NAPS2.EtoForms.Mac;

public class MacIconProvider : IIconProvider
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        { "control_play_blue", "play" },
        { "blueprints", "list.bullet" },
        { "folder_picture", "folder" },
        { "save", "square.and.arrow.down" },
        { "viewfinder", "viewfinder" },
        { "arrow_rotate_anticlockwise", "arrow.counterclockwise" },
        { "arrow_rotate_anticlockwise_small", "arrow.counterclockwise" },
        { "arrow_rotate_clockwise_small", "arrow.clockwise" },
        { "arrow_switch_small", "arrow.2.squarepath" },
        { "arrow_up_small", "arrow.up" },
        { "arrow_down_small", "arrow.down" },
        { "arrow_left", "arrow.left" },
        { "arrow_right", "arrow.right" },
        { "transform_crop", "crop" },
        { "contrast_with_sun", "sun.max" },
        { "color_management", "paintpalette" },
        { "contrast_high", "circle.righthalf.filled" },
        { "sharpen", "rhombus" },
        { "cross", "trash" },
        // TODO: Probably just use the save icon for these
        { "file_extension_pdf", "doc.richtext" },
        { "pictures", "photo" }
    };

    private readonly DefaultIconProvider _defaultIconProvider;

    public MacIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Image GetIcon(string name)
    {
        // TODO: Fix names (like "save") that have no non-mac image and will break on macOS 10.15
        if (OperatingSystem.IsMacOSVersionAtLeast(11) && IconMap.ContainsKey(name))
        {
            var symbol = NSImage.GetSystemSymbol(IconMap[name], null);
            if (symbol != null)
            {
                return new Bitmap(new BitmapHandler(symbol));
            }
        }
        return _defaultIconProvider.GetIcon(name);
    }
}