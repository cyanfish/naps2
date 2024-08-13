using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutColumn : LayoutLine
{
    public LayoutColumn(LayoutElement[] children) : base(children)
    {
    }

    public LayoutColumn(LayoutColumn original, Padding? padding = null, int? spacing = null, int? labelSpacing = null,
        int? spacingAfter = null, bool? scale = null, bool? aligned = null, LayoutVisibility? visibility = null)
        : base(original.Children)
    {
        Padding = padding ?? original.Padding;
        Spacing = spacing ?? original.Spacing;
        LabelSpacing = spacing ?? original.LabelSpacing;
        SpacingAfter = spacingAfter ?? original.SpacingAfter;
        Scale = scale ?? original.Scale;
        Aligned = aligned ?? original.Aligned;
        Visibility = visibility ?? original.Visibility;
        Width = original.Width;
        Height = original.Height;
    }

    protected int? LabelSpacing { get; init; }

    protected override bool IsOrthogonalTo(LayoutLine other) => other is LayoutRow;

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

    protected override int GetSpacingCore(int i, LayoutContext context)
    {
        if (i < Children.Count - 1 && Children[i] is LayoutControl { Control: Label })
        {
            return Children[i].SpacingAfter ?? LabelSpacing ?? context.DefaultLabelSpacing;
        }
        return base.GetSpacingCore(i, context);
    }

    protected override float GetBreadth(SizeF size) => size.Width;
    protected override float GetLength(SizeF size) => size.Height;
    protected override SizeF GetSize(float length, float breadth) => new(breadth, length);
}