namespace NAPS2.EtoForms.Mac;

public class MacDarkModeProvider : IDarkModeProvider
{
    public bool IsDarkModeEnabled =>
        NSUserDefaults.StandardUserDefaults.StringForKey("AppleInterfaceStyle") == "Dark";

    // TODO: Is it possible to detect the change?
#pragma warning disable CS0067
    public event EventHandler? DarkModeChanged;
#pragma warning restore CS0067
}