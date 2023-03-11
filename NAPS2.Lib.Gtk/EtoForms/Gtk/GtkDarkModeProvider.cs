namespace NAPS2.EtoForms.Gtk;

public class GtkDarkModeProvider : IDarkModeProvider
{
    public bool IsDarkModeEnabled { get; }

    public event EventHandler? DarkModeChanged;
}