using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public abstract class LayoutElement
{
    internal const bool DEBUG_LAYOUT = false;

    protected static LayoutElement[] ExpandChildren(LayoutElement[] children)
    {
        return children.SelectMany(x => x is ExpandLayoutElement expand ? expand.Children : new[] { x })
            .Where(c => c is not SkipLayoutElement).ToArray();
    }

    protected internal bool XScale { get; set; }
    protected internal bool YScale { get; set; }
    protected internal LayoutAlignment Alignment { get; set; }

    public static implicit operator LayoutElement(Control control) =>
        new ControlWithLayoutAttributes(control);

    public abstract void DoLayout(LayoutContext context, RectangleF bounds);

    public abstract SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds);
}