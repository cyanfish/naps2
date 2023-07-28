using Gtk;

namespace NAPS2.EtoForms.Gtk;

public class GtkDarkModeProvider : IDarkModeProvider
{
    // We need a real style context attached to the window, so it needs to be injected externally
    public StyleContext? StyleContext { get; set; }

    public bool IsDarkModeEnabled
    {
        get
        {
            var settings = Settings.GetForScreen(Gdk.Screen.Default);
            bool isDarkByStyleContext = false;
            if (StyleContext != null)
            {
                var color = StyleContext.GetColor(StateFlags.Normal);
                isDarkByStyleContext = color is { Red: > 0.5, Green: > 0.5, Blue: > 0.5 };
            }
            return settings.ApplicationPreferDarkTheme || isDarkByStyleContext;
        }
    }

    // Not sure if it's possible to detect live changes with Gtk
#pragma warning disable CS0067
    public event EventHandler? DarkModeChanged;
#pragma warning restore CS0067
}