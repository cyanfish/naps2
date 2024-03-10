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
        { "diskette", "square.and.arrow.down" },
        { "zoom", "viewfinder" },
        { "zoom_small", "viewfinder" },
        { "arrow_rotate_anticlockwise", "arrow.counterclockwise" },
        { "arrow_rotate_anticlockwise_small", "arrow.counterclockwise" },
        { "arrow_rotate_clockwise_small", "arrow.clockwise" },
        { "arrow_switch_small", "arrow.2.squarepath" },
        { "arrow_up_small", "arrow.up" },
        { "arrow_down_small", "arrow.down" },
        { "arrow_left", "arrow.left" },
        { "arrow_right", "arrow.right" },
        { "transform_crop", "crop" },
        { "weather_sun", "sun.max" },
        { "contrast_with_sun", "sun.max" },
        { "color_management", "paintpalette" },
        { "color_wheel", "paintpalette" },
        { "color_gradient", "square.righthalf.filled" },
        { "contrast", "circle.righthalf.filled" },
        { "contrast_high", "circle.righthalf.filled" },
        { "sharpen", "rhombus" },
        { "cross", "trash" },
        { "file_extension_pdf", "doc.richtext" },
        { "pictures", "photo" },
        { "document", "doc.text" },
        { "split", "squareshape.split.2x2.dotted" },
        { "combine", "rectangle.grid.1x2" }
    };

    private readonly DefaultIconProvider _defaultIconProvider;

    public MacIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Image? GetIcon(string name)
    {
        if (!OperatingSystem.IsMacOSVersionAtLeast(11) && name == "arrow_rotate_anticlockwise")
        {
            // TODO: Verify this fixes the rotate menu on macOS 10.15
            // TODO: Also maybe map other icons to 16x16 versions (e.g. control_play_blue) for better rendering
            return _defaultIconProvider.GetIcon("arrow_rotate_anticlockwise_small");
        }
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