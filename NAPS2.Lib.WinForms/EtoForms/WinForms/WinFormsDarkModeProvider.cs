using System.Windows.Forms;
using Microsoft.Win32;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsDarkModeProvider : IDarkModeProvider
{
    private bool? _value;

    public WinFormsDarkModeProvider()
    {
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    public bool IsDarkModeEnabled => _value ??= ReadDarkMode();

    private bool ReadDarkMode()
    {
        try
        {
            using var key =
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            return Equals(key?.GetValue("AppsUseLightTheme"), 0);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public event EventHandler? DarkModeChanged;

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        var newValue = ReadDarkMode();
        if (newValue != _value)
        {
            _value = newValue;
            // WinForms dark mode is experimental
#pragma warning disable WFO5001
            Application.SetColorMode(newValue ? SystemColorMode.Dark : SystemColorMode.Classic);
#pragma warning restore WFO5001
            DarkModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}