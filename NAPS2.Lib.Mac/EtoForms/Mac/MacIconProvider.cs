using Eto.Drawing;
using Eto.Mac.Drawing;

namespace NAPS2.EtoForms.Mac;

public class MacIconProvider : IIconProvider
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        { "control_play_blue", "play" },
        { "add", "plus.circle" },
        { "pencil", "pencil" },
        { "cross", "trash" },
        { "cross_small", "xmark" },
        { "accept", "checkmark.circle" },
        { "wireless16", "wifi" },
        { "blueprints", "list.bullet" },
        { "folder_picture", "folder" },
        { "diskette", "square.and.arrow.down" },
        { "zoom", "viewfinder" },
        { "arrow_rotate_anticlockwise", "arrow.counterclockwise" },
        { "arrow_rotate_clockwise", "arrow.clockwise" },
        { "arrow_switch", "arrow.2.squarepath" },
        { "arrow_up", "arrow.up" },
        { "arrow_down", "arrow.down" },
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
        { "file_extension_pdf", "doc.richtext" },
        { "pictures", "photo" },
        { "document", "doc.text" },
        { "split", "squareshape.split.2x2.dotted" },
        { "text_align_justify", "text.justify" },
        { "large_tiles", "square.grid.2x2" },
        { "exclamation", "exclamationmark.triangle" },
        { "application_side_list", "sidebar.left" },
        // TODO: Consider these
        // { "ask", "questionmark" },
        // { "network_ip", "wifi.router" },
    };

    private readonly DefaultIconProvider _defaultIconProvider;

    public MacIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Bitmap? GetIcon(string name, bool oversized = false)
    {
        if (!OperatingSystem.IsMacOSVersionAtLeast(11) && name == "arrow_rotate_anticlockwise")
        {
            // TODO: Verify this fixes the rotate menu on macOS 10.15
            // TODO: Also maybe map other icons to 16x16 versions (e.g. control_play_blue) for better rendering
            return _defaultIconProvider.GetIcon("arrow_rotate_anticlockwise_small");
        }
        if (OperatingSystem.IsMacOSVersionAtLeast(11))
        {
            if (!IconMap.ContainsKey(name) && name.EndsWith("_small"))
            {
                name = name.Substring(0, name.Length - 6);
            }
            if (IconMap.ContainsKey(name))
            {
                var symbol = NSImage.GetSystemSymbol(IconMap[name], null);
                if (symbol != null)
                {
                    if (oversized)
                    {
                        symbol = symbol.GetImage(NSImageSymbolConfiguration.Create(32, 0.1));
                    }
                    return new Bitmap(new BitmapHandler(symbol));
                }
            }
        }
        return _defaultIconProvider.GetIcon(name);
    }
}