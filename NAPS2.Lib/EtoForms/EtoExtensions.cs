using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms;

public static class EtoExtensions
{
    public static Icon ToEtoIcon(this byte[] bytes) => new(new MemoryStream(bytes));

    public static Bitmap ToEtoImage(this byte[] bytes) => new(bytes);
    public static Bitmap ToEtoImage(this IMemoryImage image) => EtoPlatform.Current.ToBitmap(image);

    public static MessageBoxType ToEto(this MessageBoxIcon icon)
    {
        return icon switch
        {
            MessageBoxIcon.Information => MessageBoxType.Information,
            MessageBoxIcon.Warning => MessageBoxType.Warning,
            _ => MessageBoxType.Information // TODO: Default type with no icon?
        };
    }

    public static DockPosition ToEto(this DockStyle dock)
    {
        return dock switch
        {
            DockStyle.Bottom => DockPosition.Bottom,
            DockStyle.Left => DockPosition.Left,
            DockStyle.Right => DockPosition.Right,
            _ => DockPosition.Top
        };
    }

    public static void AddItems(this ContextMenu contextMenu, params MenuItem[] menuItems)
    {
        contextMenu.Items.AddRange(menuItems);
    }
}