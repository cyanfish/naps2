using System.Windows.Forms;
using NAPS2.Platform.Windows;

namespace NAPS2.EtoForms.WinForms;

public static class ListViewNative
{
    public static void SetListSpacing(ListView list, int hspacing, int vspacing)
    {
        const int LVM_FIRST = 0x1000;
        const int LVM_SETICONSPACING = LVM_FIRST + 53;
        Win32.SendMessage(list.Handle, LVM_SETICONSPACING, IntPtr.Zero,
            (IntPtr) (int) (((ushort) hspacing) | (uint) (vspacing << 16)));
    }
}