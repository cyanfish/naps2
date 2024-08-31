using System.Drawing;
using System.Reflection;
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
            Invoker.Current.Invoke(ClearCachedBrushesAndPens);
            DarkModeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // Workaround for https://github.com/dotnet/winforms/issues/12027
    private void ClearCachedBrushesAndPens()
    {
        var threadData = (IDictionary<object, object?>) typeof(SystemBrushes).Assembly.GetType("System.Drawing.Gdip")!
            .GetProperty("ThreadData", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        var systemBrushesKey = typeof(SystemBrushes)
            .GetField("s_systemBrushesKey", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        var systemPensKey = typeof(SystemPens)
            .GetField("s_systemPensKey", BindingFlags.Static | BindingFlags.NonPublic)!
            .GetValue(null)!;

        threadData[systemBrushesKey] = null;
        threadData[systemPensKey] = null;
    }
}