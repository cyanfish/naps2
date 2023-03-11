using System.Windows.Forms;
using Microsoft.Win32;

namespace NAPS2.EtoForms.WinForms;

public class WinFormsDarkModeProvider : IDarkModeProvider, IMessageFilter
{
    private const int WM_SETTINGCHANGE = 0x1A;
    private const int WM_REFLECT = 0x2000;

    private bool? _value;

    public WinFormsDarkModeProvider()
    {
        Application.AddMessageFilter(this);
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

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg is WM_SETTINGCHANGE or (WM_SETTINGCHANGE | WM_REFLECT))
        {
            // TODO: Maybe we can narrow down the changed setting based on lParam?
            // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-settingchange
            _value = null;
            DarkModeChanged?.Invoke(this, EventArgs.Empty);
        }
        return false;
    }
}