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
        bool inOverlay = false;
        foreach (var child in Children)
        {
            child.DoLayout(context with { InOverlay = inOverlay }, bounds);
            inOverlay = true;
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        bool inOverlay = false;
        SizeF size = SizeF.Empty;
        foreach (var child in Children)
        {
            var childSize = child.GetPreferredSize(context with { InOverlay = inOverlay }, parentBounds);
            size = SizeF.Max(size, childSize);
            inOverlay = true;
        }
        return size;
    }
}