using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms;

public static class EtoExtensions
{
    public static DynamicRow AddSeparateRow(this DynamicLayout layout, params ControlWithLayoutAttributes[] controls)
    {
        layout.BeginVertical();
        var row = layout.BeginHorizontal();
        layout.AddAll(controls);
        layout.EndHorizontal();
        layout.EndVertical();
        return row;
    }
        
    public static void AddAll(this DynamicLayout layout, params ControlWithLayoutAttributes[] controls)
    {
        foreach (var control in controls)
        {
            layout.Add(control);
        }
    }

    public static void Add(this DynamicLayout layout, ControlWithLayoutAttributes control) =>
        control.AddTo(layout);

    public static Icon ToEtoIcon(this byte[] bytes) => new(new MemoryStream(bytes));

    public static Bitmap ToEtoImage(this byte[] bytes) => new(bytes);
    public static Bitmap ToEtoImage(this IMemoryImage image) => EtoPlatform.Current.ToBitmap(image);

    public static ControlWithLayoutAttributes Width(this Control control, int width) =>
        new ControlWithLayoutAttributes(control, width: width);
    public static ControlWithLayoutAttributes Height(this Control control, int height) =>
        new ControlWithLayoutAttributes(control, height: height);
    public static ControlWithLayoutAttributes Center(this Control control) =>
        new ControlWithLayoutAttributes(control, center: true);
    public static ControlWithLayoutAttributes XScale(this Control control) =>
        new ControlWithLayoutAttributes(control, xScale: true);
    public static ControlWithLayoutAttributes YScale(this Control control) =>
        new ControlWithLayoutAttributes(control, yScale: true);
    public static ControlWithLayoutAttributes AutoSize(this Control control) =>
        new ControlWithLayoutAttributes(control, autoSize: true);
    public static ControlWithLayoutAttributes Padding(this Control control, Padding padding) =>
        new ControlWithLayoutAttributes(control, padding: padding);
    public static ControlWithLayoutAttributes Padding(this Control control, int all) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(all));
    public static ControlWithLayoutAttributes Padding(this Control control, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(left, top, right, bottom));
    public static ControlWithLayoutAttributes Spacing(this Control control, Size spacing) =>
        new ControlWithLayoutAttributes(control, spacing: spacing);
        
    public static ControlWithLayoutAttributes Width(this ControlWithLayoutAttributes control, int width) =>
        new ControlWithLayoutAttributes(control, width: width);
    public static ControlWithLayoutAttributes Height(this ControlWithLayoutAttributes control, int height) =>
        new ControlWithLayoutAttributes(control, height: height);
    public static ControlWithLayoutAttributes Center(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, center: true);
    public static ControlWithLayoutAttributes XScale(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, xScale: true);
    public static ControlWithLayoutAttributes YScale(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, yScale: true);
    public static ControlWithLayoutAttributes AutoSize(this ControlWithLayoutAttributes control) =>
        new ControlWithLayoutAttributes(control, autoSize: true);
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, Padding padding) =>
        new ControlWithLayoutAttributes(control, padding: padding);
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, int all) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(all));
    public static ControlWithLayoutAttributes Padding(this ControlWithLayoutAttributes control, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new ControlWithLayoutAttributes(control, padding: new Padding(left, top, right, bottom));
    public static ControlWithLayoutAttributes Spacing(this ControlWithLayoutAttributes control, Size spacing) =>
        new ControlWithLayoutAttributes(control, spacing: spacing);
        
    public static LayoutColumn Padding(this LayoutColumn column, Padding padding) =>
        new LayoutColumn(column, padding: padding);
    public static LayoutColumn Padding(this LayoutColumn column, int all) =>
        new LayoutColumn(column, padding: new Padding(all));
    public static LayoutColumn Padding(this LayoutColumn column, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new LayoutColumn(column, padding: new Padding(left, top, right, bottom));
    public static LayoutColumn Spacing(this LayoutColumn column, Size spacing) =>
        new LayoutColumn(column, spacing: spacing);
    public static LayoutColumn Spacing(this LayoutColumn column, int xSpacing, int ySpacing) =>
        new LayoutColumn(column, spacing: new Size(xSpacing, ySpacing));
    public static LayoutColumn XScale(this LayoutColumn column) =>
        new LayoutColumn(column, xScale: true);
    public static LayoutColumn YScale(this LayoutColumn column) =>
        new LayoutColumn(column, yScale: true);
        
    public static LayoutRow YScale(this LayoutRow row) =>
        new LayoutRow(row, yScale: true);
    public static LayoutRow Aligned(this LayoutRow row) =>
        new LayoutRow(row, aligned: true);

    public static void AddItems(this ContextMenu contextMenu, params MenuItem[] menuItems)
    {
        contextMenu.Items.AddRange(menuItems);
    }
}