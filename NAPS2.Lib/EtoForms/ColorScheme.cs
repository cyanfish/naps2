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

    private bool DarkMode => _darkModeProvider.IsDarkModeEnabled;

    public Color ForegroundColor => DarkMode ? Colors.White : Colors.Black;

    public Color BackgroundColor => DarkMode ? VeryDarkGray : Colors.White;

    public Color SeparatorColor => DarkMode ? MidGray : LightGray;

    public Color BorderColor => DarkMode ? LightGray : Colors.Black;

    public Color CropColor => DarkMode ? HighlightBlue : Colors.Black;

    public Color NotificationBackgroundColor => DarkMode ? Color.FromRgb(0x323232) : Color.FromRgb(0xf2f2f2);
    
    public Color NotificationBorderColor => DarkMode ? Color.FromRgb(0x606060) : Color.FromRgb(0xb2b2b2);

    public event EventHandler? ColorSchemeChanged;
}