using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using NAPS2.Platform.Windows;

namespace NAPS2.EtoForms.WinForms;

/// <summary>
/// Enables larger thumbnails in System.Windows.Forms.ListView via a reflection hack.
/// </summary>
public static class WinFormsHacks
{
    private static readonly FieldInfo? ImageSizeField;
    private static readonly MethodInfo? PerformRecreateHandleMethod;
    private static readonly MethodInfo? ControlSetStyle =
        typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);

    static WinFormsHacks()
    {
        ImageSizeField = typeof(ImageList).GetField("_imageSize", BindingFlags.Instance | BindingFlags.NonPublic);
        PerformRecreateHandleMethod = typeof(ImageList).GetMethod("PerformRecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);

        if (ImageSizeField == null || PerformRecreateHandleMethod == null)
        {
            // No joy, just be happy enough with 256
            ThumbnailSizes.MAX_SIZE = 256;
        }
    }

    public static void SetImageSize(ImageList imageList, Size size)
    {
        if (ImageSizeField != null && PerformRecreateHandleMethod != null)
        {
            ImageSizeField.SetValue(imageList, size);
            PerformRecreateHandleMethod.Invoke(imageList, []);
        }
        else
        {
            imageList.ImageSize = size;
        }
    }

    public static void SetControlStyle(Control control, ControlStyles style, bool value)
    {
        if (ControlSetStyle == null)
        {
            Log.Error("Control.SetStyle not available");
        }
        else
        {
            ControlSetStyle.Invoke(control, new object[] { style, value });
        }
    }

    public static void SetListSpacing(ListView list, int hspacing, int vspacing)
    {
        const int LVM_FIRST = 0x1000;
        const int LVM_SETICONSPACING = LVM_FIRST + 53;
        Win32.SendMessage(list.Handle, LVM_SETICONSPACING, IntPtr.Zero,
            (IntPtr) (int) (((ushort) hspacing) | (uint) (vspacing << 16)));
    }
}