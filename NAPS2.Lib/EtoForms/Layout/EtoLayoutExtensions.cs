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

    public static LayoutControl Width(this Control control, int? width) =>
        new LayoutControl(control, width: width);

    public static LayoutControl MinWidth(this Control control, int minWidth) =>
        new LayoutControl(control, minWidth: minWidth);

    public static LayoutControl MaxWidth(this Control control, int maxWidth) =>
        new LayoutControl(control, maxWidth: maxWidth);

    public static LayoutControl Height(this Control control, int? height) =>
        new LayoutControl(control, height: height);

    public static LayoutControl MinHeight(this Control control, int minHeight) =>
        new LayoutControl(control, minHeight: minHeight);

    public static LayoutControl MaxHeight(this Control control, int maxHeight) =>
        new LayoutControl(control, maxHeight: maxHeight);

    public static LayoutControl Size(this Control control, int width, int height) =>
        new LayoutControl(control, width: width, height: height);

    public static LayoutControl NaturalSize(this Control control, int width, int height) =>
        new LayoutControl(control, naturalWidth: width, naturalHeight: height);

    public static LayoutControl NaturalWidth(this Control control, int width) =>
        new LayoutControl(control, naturalWidth: width);

    public static LayoutControl NaturalHeight(this Control control, int height) =>
        new LayoutControl(control, naturalHeight: height);

    public static LayoutControl Scale(this Control control) =>
        new LayoutControl(control, scale: true);

    public static LayoutControl SpacingAfter(this Control control, int spacingAfter) =>
        new LayoutControl(control, spacingAfter: spacingAfter);

    public static LayoutControl AlignCenter(this Control control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Center);

    public static LayoutControl AlignLeading(this Control control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Leading);

    public static LayoutControl AlignTrailing(this Control control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Trailing);

    public static LayoutControl Align(this Control control, LayoutAlignment alignment) =>
        new LayoutControl(control, alignment: alignment);

    public static LayoutControl Padding(this Control control, Padding padding) =>
        new LayoutControl(control, padding: padding);

    public static LayoutControl Padding(this Control control, int all) =>
        new LayoutControl(control, padding: new Padding(all));

    public static LayoutControl Padding(this Control control, int left = 0, int top = 0, int right = 0,
        int bottom = 0) =>
        new LayoutControl(control, padding: new Padding(left, top, right, bottom));

    public static LayoutControl Visible(this Control control, LayoutVisibility? visibility) =>
        new LayoutControl(control, visibility: visibility);

    /// <summary>
    /// Wraps the given label. The way this works is that is uses the specified width to calculate the allocated height,
    /// which remains static (so e.g. if you're resizing a form, the form height isn't dependent on its width). If
    /// wrapping happens at that width, it becomes the minimum width (as otherwise the label could exceed its vertical
    /// bounds). If wrapping doesn't happen, the text width is the minimum width. Note that depending on how the control
    /// is laid out in the form, you might need to set a Width or MaxWidth constraint for wrapping to actually happen.
    /// </summary>
    /// <param name="label"></param>
    /// <param name="defaultWidth"></param>
    /// <returns></returns>
    public static LayoutControl DynamicWrap(this Label label, int defaultWidth)
    {
        if (defaultWidth == 0)
        {
            return label;
        }
        label.Wrap = WrapMode.Word;
        return new LayoutControl(label, wrapDefaultWidth: defaultWidth);
    }

    public static LayoutControl Width(this LayoutControl control, int width) =>
        new LayoutControl(control, width: width);

    public static LayoutControl MinWidth(this LayoutControl control, int minWidth) =>
        new LayoutControl(control, minWidth: minWidth);

    public static LayoutControl MaxWidth(this LayoutControl control, int maxWidth) =>
        new LayoutControl(control, maxWidth: maxWidth);

    public static LayoutControl Height(this LayoutControl control, int height) =>
        new LayoutControl(control, height: height);

    public static LayoutControl MinHeight(this LayoutControl control, int minHeight) =>
        new LayoutControl(control, minHeight: minHeight);

    public static LayoutControl MaxHeight(this LayoutControl control, int maxHeight) =>
        new LayoutControl(control, maxHeight: maxHeight);

    public static LayoutControl NaturalSize(this LayoutControl control, int width, int height) =>
        new LayoutControl(control, naturalWidth: width, naturalHeight: height);

    public static LayoutControl NaturalWidth(this LayoutControl control, int width) =>
        new LayoutControl(control, naturalWidth: width);

    public static LayoutControl NaturalHeight(this LayoutControl control, int height) =>
        new LayoutControl(control, naturalHeight: height);

    public static LayoutControl Scale(this LayoutControl control) =>
        new LayoutControl(control, scale: true);

    public static LayoutControl SpacingAfter(this LayoutControl control, int spacingAfter) =>
        new LayoutControl(control, spacingAfter: spacingAfter);

    public static LayoutControl AlignCenter(this LayoutControl control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Center);

    public static LayoutControl AlignLeading(this LayoutControl control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Leading);

    public static LayoutControl AlignTrailing(this LayoutControl control) =>
        new LayoutControl(control, alignment: LayoutAlignment.Trailing);

    public static LayoutControl Align(this LayoutControl control, LayoutAlignment alignment) =>
        new LayoutControl(control, alignment: alignment);

    public static LayoutControl Padding(this LayoutControl control, Padding padding) =>
        new LayoutControl(control, padding: padding);

    public static LayoutControl Padding(this LayoutControl control, int all) =>
        new LayoutControl(control, padding: new Padding(all));

    public static LayoutControl Padding(this LayoutControl control, int left = 0, int top = 0, int right = 0,
        int bottom = 0) =>
        new LayoutControl(control, padding: new Padding(left, top, right, bottom));

    public static LayoutControl Visible(this LayoutControl control, LayoutVisibility? visibility) =>
        new LayoutControl(control, visibility: visibility);

    public static LayoutColumn Padding(this LayoutColumn column, Padding padding) =>
        new LayoutColumn(column, padding: padding);

    public static LayoutColumn Padding(this LayoutColumn column, int all) =>
        new LayoutColumn(column, padding: new Padding(all));

    public static LayoutColumn Padding(this LayoutColumn column, int left = 0, int top = 0, int right = 0,
        int bottom = 0) =>
        new LayoutColumn(column, padding: new Padding(left, top, right, bottom));

    public static LayoutColumn Spacing(this LayoutColumn column, int spacing) =>
        new LayoutColumn(column, spacing: spacing);

    public static LayoutColumn SpacingAfter(this LayoutColumn column, int spacingAfter) =>
        new LayoutColumn(column, spacingAfter: spacingAfter);

    public static LayoutColumn Scale(this LayoutColumn column) =>
        new LayoutColumn(column, scale: true);

    public static LayoutColumn Aligned(this LayoutColumn column) =>
        new LayoutColumn(column, aligned: true);

    public static LayoutColumn Visible(this LayoutColumn column, LayoutVisibility visibility) =>
        new LayoutColumn(column, visibility: visibility);

    public static LayoutRow Padding(this LayoutRow row, Padding padding) =>
        new LayoutRow(row, padding: padding);

    public static LayoutRow Padding(this LayoutRow row, int all) =>
        new LayoutRow(row, padding: new Padding(all));

    public static LayoutRow Padding(this LayoutRow row, int left = 0, int top = 0, int right = 0, int bottom = 0) =>
        new LayoutRow(row, padding: new Padding(left, top, right, bottom));

    public static LayoutRow Spacing(this LayoutRow row, int spacing) =>
        new LayoutRow(row, spacing: spacing);

    public static LayoutRow SpacingAfter(this LayoutRow row, int spacingAfter) =>
        new LayoutRow(row, spacingAfter: spacingAfter);

    public static LayoutRow Scale(this LayoutRow row) =>
        new LayoutRow(row, scale: true);

    public static LayoutRow Aligned(this LayoutRow row) =>
        new LayoutRow(row, aligned: true);

    public static LayoutRow Visible(this LayoutRow row, LayoutVisibility visibility) =>
        new LayoutRow(row, visibility: visibility);

    public static LayoutOverlay Scale(this LayoutOverlay overlay) =>
        new LayoutOverlay(overlay, scale: true);

    public static LayoutElement Expand(this IEnumerable<LayoutElement> elements) =>
        new ExpandLayoutElement(elements.ToArray());

    public static LayoutElement Expand(this IEnumerable<Control> elements) =>
        new ExpandLayoutElement(elements.Select(x => (LayoutElement) x).ToArray());

    public static void AddItems(this ContextMenu contextMenu, params MenuItem[] menuItems)
    {
        contextMenu.Items.AddRange(menuItems);
    }
}