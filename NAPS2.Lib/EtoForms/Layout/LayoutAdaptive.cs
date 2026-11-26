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
    private readonly LayoutVisibility _shown = new(true);
    private bool _isNarrow;
    private bool _isOtherHidden;
    private float _wideWidth;

    public LayoutAdaptive(LayoutElement wide, LayoutElement narrow) : base([wide, narrow])
    {
        _wide = wide;
        _narrow = narrow;
    }

    private LayoutElement Current => _isNarrow ? _narrow : _wide;

    // A control can be hidden in one arrangement and not the other, so the current one always needs a visibility for
    // the control to be shown again
    private LayoutContext GetCurrentContext(LayoutContext context) =>
        context with { ParentVisibility = context.ParentVisibility ?? _shown };

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        if (!IsVisibleInContext(context))
        {
            _wide.DoLayout(context, bounds);
            _narrow.DoLayout(context, bounds);
            _isOtherHidden = false;
            return;
        }
        context = GetCurrentContext(context);
        if (!_isNarrow)
        {
            // While narrow we use the last known width, as controls that are hidden can't be measured
            _wideWidth = _wide.GetPreferredSize(context, bounds).Width;
        }
        bool isNarrow = _wideWidth > bounds.Width;
        if (isNarrow != _isNarrow)
        {
            // Our parent has measured the other arrangement, so these bounds are the wrong size. Laying out with them
            // would e.g. stretch Mac buttons, which then keep that size as their preferred size.
            _isNarrow = isNarrow;
            _isOtherHidden = false;
            context.Invalidate();
            return;
        }
        if (!_isOtherHidden)
        {
            // Controls that are only in the other arrangement need to be hidden. This also hides the ones in both until
            // the current arrangement is laid out.
            var other = _isNarrow ? _wide : _narrow;
            other.DoLayout(context with { ParentVisibility = new LayoutVisibility(false), UseCache = false }, bounds);
            _isOtherHidden = true;
        }
        Current.DoLayout(context, bounds);
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return Current.GetPreferredSize(GetCurrentContext(context), parentBounds);
    }
}
