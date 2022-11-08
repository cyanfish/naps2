using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutColumn : LayoutLine<LayoutRow>
{
    public LayoutColumn(LayoutElement[] children) : base(children)
    {
    }

    public LayoutColumn(LayoutColumn original, Padding? padding = null, int? spacing = null, bool? xScale = null,
        bool? aligned = null)
        : base(original.Children)
    {
        Padding = padding ?? original.Padding;
        Spacing = spacing ?? original.Spacing;
        XScale = xScale ?? original.XScale;
        Aligned = aligned ?? original.Aligned;
    }

    protected override PointF UpdatePosition(PointF position, float delta)
    {
        position.Y += delta;
        return position;
    }

    protected override PointF UpdateOrthogonalPosition(PointF position, float delta)
    {
        position.X += delta;
        return position;
    }

    protected override SizeF UpdateTotalSize(SizeF size, SizeF childSize, int spacing)
    {
        size.Height += childSize.Height + spacing;
        size.Width = Math.Max(size.Width, childSize.Width);
        return size;
    }

    protected internal override bool DoesChildScale(LayoutElement child) => child.YScale;

    protected override float GetBreadth(SizeF size) => size.Width;
    protected override float GetLength(SizeF size) => size.Height;
    protected override SizeF GetSize(float length, float breadth) => new(breadth, length);
}