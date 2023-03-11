using Eto.Drawing;

namespace NAPS2.EtoForms;

public class ColorScheme
{
    private static readonly Color DarkGray = Color.FromRgb(0x262626);

    private readonly IDarkModeProvider _darkModeProvider;

    public ColorScheme(IDarkModeProvider darkModeProvider)
    {
        _darkModeProvider = darkModeProvider;
        _darkModeProvider.DarkModeChanged += (_, _) => ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public Color ForegroundColor => _darkModeProvider.IsDarkModeEnabled ? Colors.White : Colors.Black;

    public Color BackgroundColor => _darkModeProvider.IsDarkModeEnabled ? DarkGray : Colors.White;

    public event EventHandler? ColorSchemeChanged;
}