using Eto.Drawing;

namespace NAPS2.EtoForms;

public class ColorScheme
{
    private static readonly Color VeryDarkGray = Color.FromRgb(0x262626);
    private static readonly Color MidGray = Color.FromRgb(0x606060);
    private static readonly Color LightGray = Color.FromRgb(0xdddddd);
    private static readonly Color HighlightBlue = Color.FromRgb(0x007bff);

    private readonly IDarkModeProvider _darkModeProvider;

    public ColorScheme(IDarkModeProvider darkModeProvider)
    {
        _darkModeProvider = darkModeProvider;
        _darkModeProvider.DarkModeChanged += (_, _) => ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public Color ForegroundColor => _darkModeProvider.IsDarkModeEnabled ? Colors.White : Colors.Black;

    public Color BackgroundColor => _darkModeProvider.IsDarkModeEnabled ? VeryDarkGray : Colors.White;

    public Color SeparatorColor => _darkModeProvider.IsDarkModeEnabled ? MidGray : LightGray;

    public Color BorderColor => _darkModeProvider.IsDarkModeEnabled ? LightGray : Colors.Black;

    public Color CropColor => _darkModeProvider.IsDarkModeEnabled ? HighlightBlue : Colors.Black;

    public event EventHandler? ColorSchemeChanged;
}