using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

public abstract class LayoutLine<TLine, TOpposite> : LayoutContainer
    where TLine : LayoutContainer where TOpposite : LayoutContainer
{
    protected int? Spacing { get; init; }

    protected abstract PointF UpdatePosition(PointF position, float delta);

    protected abstract SizeF UpdateTotalSize(SizeF size, SizeF childSize, int spacing);

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        var childContext = GetChildContext(context, bounds);
        var cellLengths = Aligned ? context.CellLengths : null;
        var cellScaling = Aligned ? context.CellScaling : null;
        if (cellLengths == null || cellScaling == null)
        {
            cellLengths = new List<float>();
            cellScaling = new List<bool>();
            foreach (var child in Children)
            {
                cellLengths.Add(GetLength(child.GetPreferredSize(childContext, bounds)));
                cellScaling.Add(DoesChildScale(child));
            }
        }

        var spacing = Spacing ?? context.DefaultSpacing;

        var excess = GetLength(bounds.Size) - cellLengths.Sum() - spacing * (Children.Length - 1);
        var scaleCount = cellScaling.Count(scales => scales);
        var (scaleAmount, scaleExtra) = Math.DivRem((int) excess, scaleCount == 0 ? Children.Length : scaleCount);

        var cellOrigin = bounds.Location;
        for (int i = 0; i < Children.Length; i++)
        {
            var child = Children[i];
            var length = cellLengths[i];
            if (cellScaling[i] || scaleCount == 0)
            {
                length += scaleAmount;
                if (scaleExtra > 0)
                {
                    length++;
                    scaleExtra--;
                }
            }
            var cellSize = GetSize(length, GetBreadth(bounds.Size));
            child.DoLayout(childContext, new RectangleF(cellOrigin, cellSize));
            cellOrigin = UpdatePosition(cellOrigin, GetLength(cellSize) + spacing);
        }
    }

    public override SizeF GetPreferredSize(LayoutContext context, RectangleF parentBounds)
    {
        var childContext = GetChildContext(context, parentBounds);
        var size = SizeF.Empty;
        var spacing = Spacing ?? context.DefaultSpacing;
        foreach (var child in Children)
        {
            var childSize = child.GetPreferredSize(childContext, parentBounds);
            size = UpdateTotalSize(size, childSize, spacing);
        }
        size = UpdateTotalSize(size, SizeF.Empty, -spacing);
        return size;
    }

    private LayoutContext GetChildContext(LayoutContext context, RectangleF bounds)
    {
        return context with
        {
            CellLengths = GetCellLengths(context, bounds),
            CellScaling = GetCellScaling(),
            Depth = context.Depth + 1
        };
    }

    private List<float> GetCellLengths(LayoutContext context, RectangleF bounds)
    {
        var cellLengths = new List<float>();
        foreach (var child in Children)
        {
            if (child is TOpposite { Aligned: true } opposite)
            {
                for (int i = 0; i < opposite.Children.Length; i++)
                {
                    if (cellLengths.Count <= i) cellLengths.Add(0);
                    // TODO: We should probably shrink the bounds if needed
                    var preferredLength = GetBreadth(opposite.Children[i].GetPreferredSize(context, bounds));
                    cellLengths[i] = Math.Max(cellLengths[i], preferredLength);
                }
            }
        }
        return cellLengths;
    }

    private List<bool> GetCellScaling()
    {
        var cellScaling = new List<bool>();
        foreach (var child in Children)
        {
            if (child is TOpposite { Aligned: true } opposite)
            {
                for (int i = 0; i < opposite.Children.Length; i++)
                {
                    if (cellScaling.Count <= i) cellScaling.Add(false);
                    cellScaling[i] = cellScaling[i] || opposite.DoesChildScale(opposite.Children[i]);
                }
            }
        }
        return cellScaling;
    }
}