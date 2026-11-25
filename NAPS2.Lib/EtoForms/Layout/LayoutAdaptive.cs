using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

/// <summary>
/// Lays out the same controls in one of two arrangements: the wide one if it fits in the available width, otherwise
/// the narrow one.
/// </summary>
public class LayoutAdaptive : LayoutContainer
{
    private readonly LayoutElement _wide;
    private readonly LayoutElement _narrow;
    private bool _isNarrow;

    public LayoutAdaptive(LayoutElement wide, LayoutElement narrow) : base([wide, narrow])
    {
        _wide = wide;
        _narrow = narrow;
    }

    private LayoutElement Current => _isNarrow ? _narrow : _wide;

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        bool isNarrow = _wide.GetPreferredSize(context, bounds).Width > bounds.Width;
        if (isNarrow != _isNarrow)
        {
            // Our parent has measured the other arrangement, so these bounds are the wrong size. Laying out with them
            // would e.g. stretch Mac buttons, which then keep that size as their preferred size.
            _isNarrow = isNarrow;
            context.Invalidate();
            return;
        }
        Current.DoLayout(context, bounds);
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return Current.GetPreferredSize(context, parentBounds);
    }
}
