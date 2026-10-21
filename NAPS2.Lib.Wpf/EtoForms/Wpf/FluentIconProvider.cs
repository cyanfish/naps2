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
    // Unlike Mac (where we map all icons), here we're only mapping the icons for small buttons
    private static readonly Dictionary<string, string> GlyphMap = new()
    {
        { "pencil", FluentIcons.Edit },
        { "add", FluentIcons.Add },
        { "control_play_blue", FluentIcons.Play },
        { "cross", FluentIcons.Delete }
    };

    private readonly DefaultIconProvider _defaultIconProvider;

    public FluentIconProvider(DefaultIconProvider defaultIconProvider)
    {
        _defaultIconProvider = defaultIconProvider;
    }

    public Bitmap? GetIcon(string name, IconScenario scenario, float scale = 1f) =>
        scenario == IconScenario.Button && GlyphMap.TryGetValue(name, out var glyph)
            ? RenderGlyph(glyph, scenario.ToSize(), scale)
            : _defaultIconProvider.GetIcon(name, scenario, scale);

    public Icon ToFormIcon(Bitmap bitmap) => new(new IconHandler(BitmapFrame.Create(bitmap.ToWpf())));

    private static Bitmap RenderGlyph(string glyph, int size, float scale)
    {
        // The Fluent text color for the current theme
        var brush = Application.Current.TryFindResource("TextFillColorPrimaryBrush") as Brush ?? Brushes.Black;
        int pixelSize = (int) Math.Round(size * scale);
        var text = new FormattedText(glyph, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            new Typeface(FluentIcons.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            pixelSize, brush, 1);
        // Center the glyph's actual shape (the font's line metrics include extra space)
        var geometry = text.BuildGeometry(new Point(0, 0));
        var bounds = geometry.Bounds;
        geometry.Transform = new TranslateTransform(
            (pixelSize - bounds.Width) / 2 - bounds.X, (pixelSize - bounds.Height) / 2 - bounds.Y);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawGeometry(brush, null, geometry);
        }
        var bitmap = new RenderTargetBitmap(pixelSize, pixelSize, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return new Bitmap(new BitmapHandler(bitmap));
    }
}