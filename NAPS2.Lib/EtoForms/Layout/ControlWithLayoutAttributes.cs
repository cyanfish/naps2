using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class ControlWithLayoutAttributes : LayoutElement
{
    private bool _isAdded;

    public ControlWithLayoutAttributes(Control? control)
    {
        Control = control;
    }

    public ControlWithLayoutAttributes(
        ControlWithLayoutAttributes control, bool? xScale = null, bool? yScale = null,
        Padding? padding = null,
        int? width = null, int? height = null,
        int? naturalWidth = null, int? naturalHeight = null,
        LayoutAlignment? alignment = null)
    {
        Control = control.Control;
        XScale = xScale ?? control.XScale;
        YScale = yScale ?? control.YScale;
        Padding = padding ?? control.Padding;
        Width = width ?? control.Width;
        Height = height ?? control.Height;
        NaturalWidth = naturalWidth ?? control.NaturalWidth;
        NaturalHeight = naturalHeight ?? control.NaturalHeight;
        Alignment = alignment ?? control.Alignment;
    }

    public static implicit operator ControlWithLayoutAttributes(Control control) =>
        new ControlWithLayoutAttributes(control);

    private Control? Control { get; }
    private Padding Padding { get; }
    private int? Width { get; }
    private int? Height { get; }
    public int? NaturalWidth { get; }
    public int? NaturalHeight { get; }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (DEBUG_LAYOUT)
        {
            var text = Control is TextControl txt ? $"\"{txt.Text}\" " : "";
            Debug.WriteLine($"{new string(' ', context.Depth)}{text}{Control?.GetType().Name ?? "ZeroSpace"} layout with bounds {bounds}");
        }
        bounds.Size = UpdateFixedDimensions(context, bounds.Size);
        if (Control != null)
        {
            var location = new PointF(bounds.X + Padding.Left, bounds.Y + Padding.Top);
            var size = new SizeF(bounds.Width - Padding.Horizontal, bounds.Height - Padding.Vertical);
            size = SizeF.Max(SizeF.Empty, size);
            EnsureIsAdded(context);
            EtoPlatform.Current.SetFrame(context.Layout, Control, Point.Round(location), Size.Round(size));
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        var size = SizeF.Empty;
        if (Control != null)
        {
            EnsureIsAdded(context);
            size = EtoPlatform.Current.GetPreferredSize(Control, parentBounds.Size);
        }
        size = UpdateFixedDimensions(context, size);
        return new SizeF(size.Width + Padding.Horizontal, size.Height + Padding.Vertical);
    }

    private SizeF UpdateFixedDimensions(LayoutContext context, SizeF size)
    {
        if (Width != null)
        {
            size.Width = Width.Value;
        }
        if (Height != null)
        {
            size.Height = Height.Value;
        }
        if (context.IsNaturalSizeQuery && NaturalWidth != null)
        {
            size.Width = Math.Max(size.Width, NaturalWidth.Value);
        }
        if (context.IsNaturalSizeQuery && NaturalHeight != null)
        {
            size.Height = Math.Max(size.Height, NaturalHeight.Value);
        }
        return size;
    }

    private void EnsureIsAdded(LayoutContext context)
    {
        if (context.IsFirstLayout && !_isAdded)
        {
            EtoPlatform.Current.AddToContainer(context.Layout, Control);
            _isAdded = true;
        }
    }
}