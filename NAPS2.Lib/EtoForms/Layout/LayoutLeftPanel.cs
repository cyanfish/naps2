using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutLeftPanel : LayoutElement
{
    private readonly LayoutElement _left;
    private readonly LayoutElement _right;
    private readonly LayoutOverlay _overlay;
    private readonly Splitter _splitter;

    private bool _isInitialized;

    public LayoutLeftPanel(LayoutElement left, LayoutElement right)
    {
        _left = left;
        _right = right;
        _splitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Panel1 = new Panel(),
            Panel2 = new Panel(),
            FixedPanel = SplitterFixedPanel.Panel1
        };
        _overlay = L.Overlay(_splitter, L.Row(left, right).Spacing(3));
    }

    public override void Materialize(LayoutContext context) => _overlay.Materialize(context);

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        var w = MeasureWidth(context, bounds, _left);
        if (_splitter.Position < w) _splitter.Position = w;
        _splitter.Panel1MinimumSize = w;
        _splitter.Panel2MinimumSize = (int) (100 * context.Scale);

        if (!_isInitialized)
        {
            _left.Width = _splitter.Position = _splitter.Panel1MinimumSize;
            _splitter.PositionChanged += (_, _) =>
            {
                if (_left.Width != _splitter.Position)
                {
                    _left.Width = _splitter.Position;
                    context.Invalidate();
                }
            };
            if (_left.Visibility is { } vis)
            {
                vis.IsVisibleChanged += (_, _) => _splitter.Visible = vis.IsVisible;
            }
            _isInitialized = true;
        }

        _overlay.DoLayout(context, bounds);
    }

    private int MeasureWidth(LayoutContext context, RectangleF bounds, LayoutElement element)
    {
        var w = element.Width;
        element.Width = null;
        var measureContext = context with
        {
            IsLayout = false,
            UseCache = false
        };
        int result = (int) element.GetPreferredSize(measureContext, bounds).Width;
        element.Width = w;
        return result;
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return _overlay.GetPreferredSize(context, parentBounds);
    }
}