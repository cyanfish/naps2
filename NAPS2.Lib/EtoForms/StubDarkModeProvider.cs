namespace NAPS2.EtoForms;

public class StubDarkModeProvider : IDarkModeProvider
{
    public bool IsDarkModeEnabled => false;

#pragma warning disable CS0067
    public event EventHandler? DarkModeChanged;
#pragma warning restore CS0067
}