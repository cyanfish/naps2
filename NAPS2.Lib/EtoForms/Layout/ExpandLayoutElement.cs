using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class ExpandLayoutElement : LayoutElement
{
    public ExpandLayoutElement(params LayoutElement[] children)
    {
        Children = children;
    }

    public LayoutElement[] Children { get; }

    public override void DoLayout(LayoutContext context, RectangleF bounds) => throw new NotSupportedException();

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds) =>
        throw new NotSupportedException();
}