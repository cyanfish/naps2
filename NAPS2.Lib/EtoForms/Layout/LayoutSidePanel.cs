using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

/// <summary>
/// Lays out a main element and a user-resizable panel to its left or right. The panel keeps its width as the window
/// resizes, and when it's hidden the main element fills the whole area.
/// </summary>
public class LayoutSidePanel : LayoutContainer
{
    private readonly LayoutElement _panel;
    private readonly bool _isRight;
    private readonly LayoutOverlay _overlay;
    private readonly int _spacing = EtoPlatform.Current.IsWinForms ? 3 : 2;

    private Func<int> _widthGetter = () => 0;
    private Action<int> _widthSetter = _ => { };
    private int? _minPanelWidth;
    private int? _minMainWidth;
    private double? _maxShownFraction;
    private bool _isInitialized;
    private bool _inLayout;
    private int _lastTotalWidth;
    // The width the user chose, and the width actually used (which may be less if the window is too narrow)
    private int _panelWidth;
    private int _effectiveWidth;
    private bool _wasPanelVisible;
    private Action? _invalidate;

    public LayoutSidePanel(LayoutElement left, LayoutElement right, bool isRight) : base([left, right])
    {
        _panel = isRight ? right : left;
        _isRight = isRight;
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
        bool panelVisible = _panel.IsVisible && _panel is not SkipLayoutElement;
        int minPanel = _minPanelWidth ?? (panelVisible ? MeasureWidth(context, bounds, _panel) : 0);
        int minMain = _minMainWidth ?? 0;
        int total = (int) bounds.Width;

        if (!_isInitialized)
        {
            _panelWidth = _widthGetter();
            Splitter.PositionChanged += SplitterPositionChanged;
            _isInitialized = true;
        }
        if (panelVisible && !_wasPanelVisible && _maxShownFraction is { } fraction)
        {
            _panelWidth = Math.Min(_widthGetter(), Math.Max(minPanel, (int) (total * fraction)));
        }
        _wasPanelVisible = panelVisible;
        _effectiveWidth = _panelWidth.Clamp(minPanel, Math.Max(minPanel, total - minMain - _spacing));
        _lastTotalWidth = total;

        if (_panel.Width != _effectiveWidth)
        {
            // Our parent has already measured the panel at its old width, so lay out again
            _panel.Width = _effectiveWidth;
            context.Invalidate();
        }
        // Changing the minimum sizes can move the splitter (e.g. when the panel is shown), which isn't a user drag
        _inLayout = true;
        int panelMinimumSize = panelVisible ? minPanel : 0;
        Splitter.Panel1MinimumSize = _isRight ? minMain : panelMinimumSize;
        Splitter.Panel2MinimumSize = _isRight ? panelMinimumSize : minMain;
        _inLayout = false;

        _overlay.DoLayout(context, bounds);

        // The splitter's panels are empty placeholders hidden behind the real content; only its grip, in the gap
        // between the two elements, is exposed. When the panel is hidden the grip is pushed off the edge.
        int position = !panelVisible ? _isRight ? total : 0
            : _isRight ? total - _effectiveWidth - _spacing : _effectiveWidth;
        _inLayout = true;
        EtoPlatform.Current.SetSplitterPosition(Splitter, position);
        _inLayout = false;
    }

    private void SplitterPositionChanged(object? sender, EventArgs e)
    {
        // Position changes while the splitter hasn't been resized to the latest layout yet (e.g. Gtk clamping the
        // position to a stale allocation) aren't user drags
        if (_inLayout || !_wasPanelVisible || Splitter.Width != _lastTotalWidth) return;
        int newWidth = _isRight ? _lastTotalWidth - Splitter.Position - _spacing : Splitter.Position;
        if (newWidth != _effectiveWidth)
        {
            _panelWidth = _effectiveWidth = newWidth;
            _panel.Width = _effectiveWidth;
            _widthSetter(_panelWidth);
            _invalidate?.Invoke();
        }
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

    public LayoutSidePanel SizeConfig(Func<int> getter, Action<int> setter, int? minPanelWidth = null,
        int? minMainWidth = null)
    {
        _widthGetter = getter;
        _widthSetter = setter;
        _minPanelWidth = minPanelWidth;
        _minMainWidth = minMainWidth;
        return this;
    }

    /// <summary>
    /// Limits the panel to a fraction of the available width when it's shown, so that it doesn't take over most of
    /// a small window. The user can still make it wider.
    /// </summary>
    public LayoutSidePanel MaxShownFraction(double fraction)
    {
        _maxShownFraction = fraction;
        return this;
    }
}
