using System.Globalization;
using System.Text;
using Eto.Drawing;
using Eto.GtkSharp.Drawing;
using GTK = Gtk;

namespace NAPS2.EtoForms.Gtk;

/// <summary>
/// Uses bundled Lucide icons (tinted with the theme's text color) instead of some of NAPS2's icons, falling back to
/// the default icons for everything else.
/// </summary>
internal class LucideIconProvider : IIconProvider
{
    private static readonly Dictionary<string, string> IconMap = new()
    {
        { "pencil", "pencil" },
        { "add", "plus" },
        { "control_play_blue", "play" },
        { "cross", "trash-2" },
        { "cancel", "x" },
        { "arrow_left", "chevron-left" },
        { "arrow_right", "chevron-right" },
        { "new_window", "external-link" },
        { "application_side_list_right", "panel-right" },
        { "arrow_out", "expand" },
        { "zoom_in", "zoom-in" },
        { "zoom_out", "zoom-out" }
    };

    private static readonly Lazy<GTK.Button> StyleSource = new(() => new GTK.Button());

    private readonly DefaultIconProvider _defaultIconProvider;

    public LucideIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Bitmap? GetIcon(string name, IconScenario scenario, float scale = 1f) =>
        (scenario == IconScenario.Button || name == "new_window") && IconMap.TryGetValue(name, out var svgName)
            ? RenderSvg(svgName, (int) (scenario.ToSize() * scale)) ??
              _defaultIconProvider.GetIcon(name, scenario, scale)
            : _defaultIconProvider.GetIcon(name, scenario, scale);

    public Icon ToFormIcon(Bitmap bitmap) => _defaultIconProvider.ToFormIcon(bitmap);

    private static Bitmap? RenderSvg(string name, int size)
    {
        using var stream = typeof(LucideIconProvider).Assembly.GetManifestResourceStream($"NAPS2.Icons.{name}.svg");
        if (stream == null) return null;
        var svg = new StreamReader(stream).ReadToEnd();
        var context = StyleSource.Value.StyleContext;
        var color = context.GetColor(context.State);
        var hex = $"#{ToByte(color.Red):x2}{ToByte(color.Green):x2}{ToByte(color.Blue):x2}";
        svg = svg.Replace("stroke=\"currentColor\"", $"stroke=\"{hex}\" opacity=\"0.75\"");
        if (size < 24)
        {
            // The icons have a 24px grid with 2px lines, so at smaller sizes we thin the lines and offset them by
            // half a pixel so they're still sharp
            var offset = (-12f / size).ToString(CultureInfo.InvariantCulture);
            svg = svg.Replace("stroke-width=\"2\"", "stroke-width=\"1.5\"")
                .Replace("viewBox=\"0 0 24 24\"", $"viewBox=\"{offset} {offset} 24 24\"");
        }
        try
        {
            return new Bitmap(new BitmapHandler(new Gdk.Pixbuf(Encoding.UTF8.GetBytes(svg), size, size)));
        }
        catch (GLib.GException)
        {
            // The SVG loader isn't available
            return null;
        }
    }

    private static int ToByte(double value) => (int) Math.Round(value * 255);
}
