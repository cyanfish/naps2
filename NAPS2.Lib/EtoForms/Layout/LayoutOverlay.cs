using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class LayoutOverlay : LayoutContainer
{
    public LayoutOverlay(IEnumerable<LayoutElement> children)
        : base(children)
    {
    }

    public LayoutOverlay(LayoutOverlay original, bool? scale = null, int? spacingAfter = null)
        : base(original.Children)
    {
        Scale = scale ?? original.Scale;
        SpacingAfter = spacingAfter ?? original.SpacingAfter;
        Width = original.Width;
        Height = original.Height;
    }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        bool inOverlay = false;
        foreach (var child in Children)
        {
            child.DoLayout(context with { InOverlay = context.InOverlay || inOverlay }, bounds);
            inOverlay = true;
        }
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        bool inOverlay = false;
        SizeF size = SizeF.Empty;
        foreach (var child in Children)
        {
            var childSize = child.GetPreferredSize(context with { InOverlay = context.InOverlay || inOverlay }, parentBounds);
            size = SizeF.Max(size, childSize);
            inOverlay = true;
        }
        if (Width != null)
        {
            size.Width = Width.Value;
        }
        if (Height != null)
        {
            size.Height = Height.Value;
        }
        return size;
    }
}