using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Eto.Wpf;
using Eto.Wpf.Drawing;
using Bitmap = Eto.Drawing.Bitmap;
using Icon = Eto.Drawing.Icon;
using Point = System.Windows.Point;

namespace NAPS2.EtoForms.Wpf;

/// <summary>
/// Uses Fluent icon font glyphs instead of some of NAPS2's icons, falling back to the default icons for everything
/// else.
/// </summary>
internal class FluentIconProvider : IIconProvider
{
    // Unlike Mac (where we map all icons), here we're only mapping the small versions (i.e. not the toolbar icons)
    private static readonly Dictionary<string, string> GlyphMap = new()
    {
        { "pencil_small", FluentIcons.Edit },
        { "add_small", FluentIcons.Add },
        { "control_play_blue_small", FluentIcons.Play },
        { "cross_small", FluentIcons.Delete }
    };

    private readonly DefaultIconProvider _defaultIconProvider;

    public FluentIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Bitmap? GetIcon(string name, float scale = 1f, bool oversized = false) =>
        GlyphMap.TryGetValue(name, out var glyph)
            ? RenderGlyph(glyph, scale)
            : _defaultIconProvider.GetIcon(name, scale, oversized);

    public Icon? GetFormIcon(string name, float scale = 1f)
    {
        using var bitmap = _defaultIconProvider.GetIcon(name, scale);
        return bitmap != null ? new Icon(new IconHandler(BitmapFrame.Create(bitmap.ToWpf()))) : null;
    }

    private static Bitmap RenderGlyph(string glyph, float scale)
    {
        // The Fluent text color for the current theme
        var brush = Application.Current.TryFindResource("TextFillColorPrimaryBrush") as Brush ?? Brushes.Black;
        int size = (int) Math.Round(16 * scale);
        var text = new FormattedText(glyph, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(FluentIcons.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            size, brush, 1);
        // Center the glyph's actual shape (the font's line metrics include extra space)
        var geometry = text.BuildGeometry(new Point(0, 0));
        var bounds = geometry.Bounds;
        geometry.Transform = new TranslateTransform(
            (size - bounds.Width) / 2 - bounds.X, (size - bounds.Height) / 2 - bounds.Y);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawGeometry(brush, null, geometry);
        }
        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return new Bitmap(new BitmapHandler(bitmap));
    }
}