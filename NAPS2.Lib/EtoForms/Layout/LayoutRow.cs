using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public class LayoutRow : LayoutLine
{
    public LayoutRow(LayoutElement[] children) : base(children)
    {
    }

    public LayoutRow(LayoutRow original, Padding? padding = null, int? spacing = null, int? spacingAfter = null,
        bool? scale = null, bool? aligned = null, LayoutVisibility? visibility = null)
        : base(original.Children)
    {
        Padding = padding ?? original.Padding;
        Spacing = spacing ?? original.Spacing;
        SpacingAfter = spacingAfter ?? original.SpacingAfter;
        Scale = scale ?? original.Scale;
        Aligned = aligned ?? original.Aligned;
        Visibility = visibility ?? original.Visibility;
        Width = original.Width;
        Height = original.Height;
    }

    protected override bool IsOrthogonalTo(LayoutLine other) => other is LayoutColumn;

    protected override PointF UpdatePosition(PointF position, float delta)
    {
        position.X += delta;
        return position;
    }

    protected override PointF UpdateOrthogonalPosition(PointF position, float delta)
    {
        position.Y += delta;
        return position;
    }

    protected override SizeF UpdateTotalSize(SizeF size, SizeF childSize, int spacing)
    {
        size.Width += childSize.Width + spacing;
        size.Height = Math.Max(size.Height, childSize.Height);
        return size;
    }

    protected override float GetBreadth(SizeF size) => size.Height;
    protected override float GetLength(SizeF size) => size.Width;
    protected override SizeF GetSize(float length, float breadth) => new(length, breadth);
}