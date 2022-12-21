using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

// Ignore unreachable code for DEBUG_LAYOUT
#pragma warning disable CS0162
public class LayoutController
{
    public const int MAX_SIZE = int.MaxValue / 2;
    private readonly Control _layout = EtoPlatform.Current.CreateContainer();
    private LayoutElement? _content;
    private Window? _window;
    private bool _firstLayout = true;
    private bool _isShown;
    private bool _layoutQueued;

    public LayoutElement? Content
    {
        get => _content;
        set
        {
            bool setting = _content == null;
            _content = value;
            DoLayout();
            if (setting)
            {
                Invalidated?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public int RootPadding { get; set; } = 10;
    public int DefaultLabelSpacing { get; set; } = 2;
    public int DefaultSpacing { get; set; } = 6;

    public void Bind(Window window)
    {
        if (_window != null) throw new InvalidOperationException();
        _window = window;
        window.Content = _layout;
        window.Shown += (_, _) =>
        {
            _isShown = true;
            DoLayout();
        };
        window.SizeChanged += (_, _) => DoLayout();
    }

    public Size GetLayoutSize(bool natural)
    {
        if (_content == null) throw new InvalidOperationException();
        var bounds = new RectangleF(0, 0, MAX_SIZE, MAX_SIZE);
        var context = GetLayoutContext() with { IsNaturalSizeQuery = natural };
        var contentSize = _content.GetPreferredSize(context, bounds);
        var padding = new SizeF(RootPadding * 2, RootPadding * 2);
        var naturalSize = Size.Ceiling(contentSize + padding);
        return naturalSize;
    }

    public void Invalidate()
    {
        _layoutQueued = true;
        Invalidated?.Invoke(this, EventArgs.Empty);
        _layoutQueued = false;
        DoLayout();
    }

    private void DoLayout()
    {
        if (_window == null || _content == null || !_isShown || _layoutQueued) return;
        // TODO: Handle added/removed things
        var size = EtoPlatform.Current.GetClientSize(_window);
        int p = RootPadding;
        var bounds = new Rectangle(p, p, size.Width - 2 * p, size.Height - 2 * p);
        if (bounds.Width < 0 || bounds.Height < 0)
        {
            return;
        }
        var context = GetLayoutContext() with { Window = _window, IsLayout = true };
        _firstLayout = false;
        if (LayoutElement.DEBUG_LAYOUT)
        {
            Debug.WriteLine("\n(((Starting layout)))");
        }
        _window.SuspendLayout();
        _content.DoLayout(context, bounds);
        _window.ResumeLayout();
        EtoPlatform.Current.SetContainerSize(_window, _layout, size, p);
    }

    private LayoutContext GetLayoutContext()
    {
        return new LayoutContext(_layout)
        {
            DefaultSpacing = DefaultSpacing,
            DefaultLabelSpacing = DefaultLabelSpacing,
            IsFirstLayout = _firstLayout,
            Invalidate = Invalidate
        };
    }

    public event EventHandler? Invalidated;
}