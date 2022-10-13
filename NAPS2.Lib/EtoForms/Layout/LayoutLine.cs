using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

/// <summary>
/// Abstract base class for LayoutColumn and LayoutRow. We use this class to generalize column and row layout logic.
/// </summary>
/// <typeparam name="TOrthogonal">The orthogonal type (e.g. LayoutRow if this is LayoutColumn).</typeparam>
public abstract class LayoutLine<TOrthogonal> : LayoutContainer
    where TOrthogonal : LayoutContainer
{
    protected int? Spacing { get; init; }

    protected abstract PointF UpdatePosition(PointF position, float delta);

    protected abstract SizeF UpdateTotalSize(SizeF size, SizeF childSize, int spacing);

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (DEBUG_LAYOUT)
        {
            Debug.WriteLine($"{new string(' ', context.Depth)}{GetType().Name} layout with bounds {bounds}");
        }
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

        var scaleCount = cellScaling.Count(scales => scales);
        if (scaleCount > 0)
        {
            // If no controls scale, then they will all take up their preferred length.
            // If some controls scale, then we take [excess = remaining space + length of all scaling controls],
            // and divide that evenly among all scaling controls so they all have equal length.
            var excess = GetLength(bounds.Size) - spacing * (Children.Length - 1);
            for (int i = 0; i < Children.Length; i++)
            {
                if (!cellScaling[i])
                {
                    excess -= cellLengths[i];
                }
            }
            // Update the lengths of scaling controls
            var scaleAmount = Math.DivRem((int) excess, scaleCount, out int scaleExtra);
            for (int i = 0; i < Children.Length; i++)
            {
                if (cellScaling[i])
                {
                    cellLengths[i] = scaleAmount + (scaleExtra-- > 0 ? 1 : 0);
                }
            }
        }

        var cellOrigin = bounds.Location;
        for (int i = 0; i < Children.Length; i++)
        {
            var cellSize = GetSize(cellLengths[i], GetBreadth(bounds.Size));
            Children[i].DoLayout(childContext, new RectangleF(cellOrigin, cellSize));
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
            if (child is TOrthogonal { Aligned: true } opposite)
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
            if (child is TOrthogonal { Aligned: true } opposite)
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