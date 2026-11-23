using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

/// <summary>
/// Lays out a main element and a user-resizable panel to its right. The panel keeps its width as the window resizes,
/// and when it's hidden the main element fills the whole area.
/// </summary>
public class LayoutRightPanel : LayoutContainer
{
    private readonly LayoutElement _right;
    private readonly LayoutOverlay _overlay;
    private readonly int _spacing = EtoPlatform.Current.IsWinForms ? 3 : 2;

    private Func<int> _widthGetter = () => 0;
    private Action<int> _widthSetter = _ => { };
    private int? _minLeftWidth;
    private int? _minRightWidth;
    private bool _isInitialized;
    private bool _inLayout;
    private int _lastTotalWidth;
    // The width the user chose, and the width actually used (which may be less if the window is too narrow)
    private int _rightWidth;
    private int _effectiveWidth;
    private Action? _invalidate;

    public LayoutRightPanel(LayoutElement left, LayoutElement right) : base([left, right])
    {
        _right = right;
        Splitter = new Splitter
        {
            Orientation = Orientation.Horizontal,
            Panel1 = new Panel(),
            Panel2 = new Panel(),
            FixedPanel = SplitterFixedPanel.Panel1
        };
        _overlay = L.Overlay(Splitter, L.Row(left, right).Spacing(_spacing));
        Scale = true;
    }

    public Splitter Splitter { get; }

    public override void Materialize(LayoutContext context) => _overlay.Materialize(context);

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        _invalidate = context.Invalidate;
        int minLeft = _minLeftWidth ?? 0;
        int minRight = _minRightWidth ?? 0;
        int total = (int) bounds.Width;

        if (!_isInitialized)
        {
            _rightWidth = _widthGetter();
            Splitter.PositionChanged += SplitterPositionChanged;
            _isInitialized = true;
        }
        _effectiveWidth = _rightWidth.Clamp(minRight, Math.Max(minRight, total - minLeft - _spacing));
        _lastTotalWidth = total;

        bool rightVisible = _right.IsVisible;
        if (_right.Width != _effectiveWidth)
        {
            // Our parent has already measured the right element at its old width, so lay out again
            _right.Width = _effectiveWidth;
            context.Invalidate();
        }
        Splitter.Panel1MinimumSize = minLeft;
        Splitter.Panel2MinimumSize = rightVisible ? minRight : 0;

        _overlay.DoLayout(context, bounds);

        // The splitter's panels are empty placeholders hidden behind the real content; only its grip, in the gap
        // between the two elements, is exposed. When the panel is hidden the grip is pushed off the edge.
        _inLayout = true;
        EtoPlatform.Current.SetSplitterPosition(Splitter, rightVisible ? total - _effectiveWidth - _spacing : total);
        _inLayout = false;
    }

    private void SplitterPositionChanged(object? sender, EventArgs e)
    {
        // Position changes while the splitter hasn't been resized to the latest layout yet (e.g. Gtk clamping the
        // position to a stale allocation) aren't user drags
        if (_inLayout || !_right.IsVisible || Splitter.Width != _lastTotalWidth) return;
        int newWidth = _lastTotalWidth - Splitter.Position - _spacing;
        if (newWidth != _effectiveWidth)
        {
            _rightWidth = _effectiveWidth = newWidth;
            _right.Width = _effectiveWidth;
            _widthSetter(_rightWidth);
            _invalidate?.Invoke();
        }
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        return _overlay.GetPreferredSize(context, parentBounds);
    }

    public LayoutRightPanel SizeConfig(Func<int> getter, Action<int> setter, int? minRightWidth = null,
        int? minLeftWidth = null)
    {
        _widthGetter = getter;
        _widthSetter = setter;
        _minRightWidth = minRightWidth;
        _minLeftWidth = minLeftWidth;
        return this;
    }
}
