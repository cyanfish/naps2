using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class LayoutOverlay : LayoutElement
{
    public LayoutOverlay(LayoutElement[] children)
    {
        Children = children;
    }

    public LayoutElement[] Children { get; set; }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        foreach (var child in Children)
        {
            child.DoLayout(context with { InOverlay = true }, bounds);
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        return Children
            .Select(child => child.GetPreferredSize(context, parentBounds))
            .Aggregate(SizeF.Max);
    }
}