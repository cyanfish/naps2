using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace NAPS2.EtoForms.WinForms;

/// <summary>
/// Enables larger thumbnails in System.Windows.Forms.ListView via a reflection hack.
/// </summary>
public static class ListViewImageSizeHack
{
    private static readonly FieldInfo? ImageSizeField;
    private static readonly MethodInfo? PerformRecreateHandleMethod;

    static ListViewImageSizeHack()
    {
        if (PlatformCompat.Runtime.SetImageListSizeOnImageCollection)
        {
            ImageSizeField = typeof(ImageList.ImageCollection).GetField("imageSize", BindingFlags.Instance | BindingFlags.NonPublic);
            PerformRecreateHandleMethod = typeof(ImageList.ImageCollection).GetMethod("RecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        else
        {
            ImageSizeField = typeof(ImageList).GetField("imageSize", BindingFlags.Instance | BindingFlags.NonPublic);
            PerformRecreateHandleMethod = typeof(ImageList).GetMethod("PerformRecreateHandle", BindingFlags.Instance | BindingFlags.NonPublic);
        }

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
            if (PlatformCompat.Runtime.SetImageListSizeOnImageCollection)
            {
                ImageSizeField.SetValue(imageList.Images, size);
                PerformRecreateHandleMethod.Invoke(imageList.Images, new object[] { });
            }
            else
            {
                ImageSizeField.SetValue(imageList, size);
                PerformRecreateHandleMethod.Invoke(imageList, new object[] { "ImageSize" });
            }
        }
        else
        {
            imageList.ImageSize = size;
        }
    }
}