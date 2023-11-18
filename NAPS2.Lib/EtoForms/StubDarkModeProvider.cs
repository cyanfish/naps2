namespace NAPS2.EtoForms;

public class StubDarkModeProvider : IDarkModeProvider
{
    public bool IsDarkModeEnabled => false;
    public event EventHandler? DarkModeChanged;
}