using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public static class EtoLayoutExtensions
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

    public static ControlWithLayoutAttributes Width(this Control control, int width) =>
        new ControlWithLayoutAttributes(control, width: width);
    public static ControlWithLayoutAttributes Height(this Control control, int height) =>
        new ControlWithLayoutAttributes(control, height: height);
    public static ControlWithLayoutAttributes Size(this Control control, int width, int height) =>
        new ControlWithLayoutAttributes(control, width: width, height: height);
    public static ControlWithLayoutAttributes NaturalSize(this Control control, int width, int height) =>
        new ControlWithLayoutAttributes(control, naturalWidth: width, naturalHeight: height);
    public static ControlWithLayoutAttributes XScale(this Control control) =>
        new ControlWithLayoutAttributes(control, xScale: true);
    public static ControlWithLayoutAttributes YScale(this Control control) =>
        new ControlWithLayoutAttributes(control, yScale: true);
    public static ControlWithLayoutAttributes AlignCenter(this Control control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Center);
    public static ControlWithLayoutAttributes AlignLeading(this Control control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Leading);
    public static ControlWithLayoutAttributes AlignTrailing(this Control control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Trailing);
    public static ControlWithLayoutAttributes Align(this Control control, LayoutAlignment alignment) =>
        new ControlWithLayoutAttributes(control, alignment: alignment);
    public static ControlWithLayoutAttributes Padding(this Control control, Padding padding) =>
        new ControlWithLayoutAttributes(control, padding: padding);
    public static ControlWithLayoutAttributes Padding(this Control control, int all) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(all));
    public static ControlWithLayoutAttributes Padding(this Control control, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(left, top, right, bottom));

    public static ControlWithLayoutAttributes Width(this ControlWithLayoutAttributes control, int width) =>
        new ControlWithLayoutAttributes(control, width: width);
    public static ControlWithLayoutAttributes Height(this ControlWithLayoutAttributes control, int height) =>
        new ControlWithLayoutAttributes(control, height: height);
    public static ControlWithLayoutAttributes XScale(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, xScale: true);
    public static ControlWithLayoutAttributes YScale(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, yScale: true);
    public static ControlWithLayoutAttributes AlignCenter(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Center);
    public static ControlWithLayoutAttributes AlignLeading(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Leading);
    public static ControlWithLayoutAttributes AlignTrailing(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, alignment: LayoutAlignment.Trailing);
    public static ControlWithLayoutAttributes Align(this ControlWithLayoutAttributes control, LayoutAlignment alignment) =>
        new ControlWithLayoutAttributes(control, alignment: alignment);
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, Padding padding) =>
        new ControlWithLayoutAttributes(control, padding: padding);
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, int all) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(all));
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(left, top, right, bottom));

    public static LayoutColumn Padding(this LayoutColumn column, Padding padding) =>
        new LayoutColumn(column, padding: padding);
    public static LayoutColumn Padding(this LayoutColumn column, int all) =>
        new LayoutColumn(column, padding: new Padding(all));
    public static LayoutColumn Padding(this LayoutColumn column, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new LayoutColumn(column, padding: new Padding(left, top, right, bottom));
    public static LayoutColumn Spacing(this LayoutColumn column, int spacing) =>
        new LayoutColumn(column, spacing: spacing);
    public static LayoutColumn XScale(this LayoutColumn column) =>
        new LayoutColumn(column, xScale: true);
    public static LayoutColumn Aligned(this LayoutColumn column) =>
        new LayoutColumn(column, aligned: true);

    public static LayoutRow Padding(this LayoutRow row, Padding padding) =>
        new LayoutRow(row, padding: padding);
    public static LayoutRow Padding(this LayoutRow row, int all) =>
        new LayoutRow(row, padding: new Padding(all));
    public static LayoutRow Padding(this LayoutRow row, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new LayoutRow(row, padding: new Padding(left, top, right, bottom));
    public static LayoutRow Spacing(this LayoutRow row, int spacing) =>
        new LayoutRow(row, spacing: spacing);
    public static LayoutRow YScale(this LayoutRow row) =>
        new LayoutRow(row, yScale: true);
    public static LayoutRow Aligned(this LayoutRow row) =>
        new LayoutRow(row, aligned: true);

    public static LayoutOverlay XScale(this LayoutOverlay overlay) =>
        new LayoutOverlay(overlay, xScale: true);
    public static LayoutOverlay YScale(this LayoutOverlay overlay) =>
        new LayoutOverlay(overlay, yScale: true);

    public static void AddItems(this ContextMenu contextMenu, params MenuItem[] menuItems)
    {
        contextMenu.Items.AddRange(menuItems);
    }
}