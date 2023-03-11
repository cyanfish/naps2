namespace NAPS2.EtoForms;

public interface IDarkModeProvider
{
    bool IsDarkModeEnabled { get; }

    event EventHandler? DarkModeChanged;
}