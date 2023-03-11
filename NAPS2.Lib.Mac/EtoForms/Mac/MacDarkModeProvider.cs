namespace NAPS2.EtoForms.Mac;

public class MacDarkModeProvider : IDarkModeProvider
{
    public bool IsDarkModeEnabled { get; }

    public event EventHandler? DarkModeChanged;
}