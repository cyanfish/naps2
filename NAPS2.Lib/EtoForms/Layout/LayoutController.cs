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
    private bool _isShown;
    private bool _layoutQueued;
    private HashSet<Control> _controlSet = new();

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

    public Control Container => _layout;

    public int RootPadding { get; set; } = 10;
    public int DefaultLabelSpacing { get; set; } = 2;
    public int DefaultSpacing { get; set; } = 6;

    public void Bind(Window window)
    {
        if (_window != null) throw new InvalidOperationException();
        _window = window;
        window.Content = _layout;
        window.LoadComplete += (_, _) =>
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
        _content.Materialize(context);
        var contentSize = _content.GetPreferredSize(context, bounds);
        var padding = new SizeF(RootPadding * 2, RootPadding * 2);
        var naturalSize = Size.Ceiling(contentSize + padding);
        return naturalSize;
    }

    public void Invalidate()
    {
        if (_layoutQueued) return;
        if (_window == null || _content == null || !_isShown)
        {
            Invoker.Current.InvokeDispatch(() => { Invalidated?.Invoke(this, EventArgs.Empty); });
            return;
        }
        _layoutQueued = true;
        Invoker.Current.InvokeDispatch(() =>
        {
            Invalidated?.Invoke(this, EventArgs.Empty);
            _layoutQueued = false;
            DoLayout();
        });
    }

    private void DoLayout()
    {
        if (_window == null || _content == null || !_isShown || _layoutQueued) return;
        var size = EtoPlatform.Current.GetClientSize(_window, true);
        int p = RootPadding;
        var bounds = new Rectangle(p, p, size.Width - 2 * p, size.Height - 2 * p);
        if (bounds.Width < 0 || bounds.Height < 0)
        {
            return;
        }
        var context = GetLayoutContext() with { Window = _window, IsLayout = true };
        if (LayoutElement.DEBUG_LAYOUT)
        {
            Debug.WriteLine("\n(((Starting layout)))");
        }
        _window.SuspendLayout();
        _content.Materialize(context);
        _content.DoLayout(context, bounds);
        RemoveControls();
        _window.ResumeLayout();
        EtoPlatform.Current.SetContainerSize(_window, _layout, size, p);
    }

    private void RemoveControls()
    {
        var newControlSet = new HashSet<Control>();
        PopulateControlSet(newControlSet, _content!);
        foreach (var removedControl in _controlSet.Except(newControlSet))
        {
            EtoPlatform.Current.RemoveFromContainer(_layout, removedControl);
        }
        _controlSet = newControlSet;
    }

    private void PopulateControlSet(HashSet<Control> controlSet, LayoutElement element)
    {
        if (element is LayoutContainer container)
        {
            foreach (var child in container.Children)
            {
                PopulateControlSet(controlSet, child);
            }
        }
        if (element is LayoutControl { Control: { } } control)
        {
            controlSet.Add(control.Control);
        }
    }

    internal LayoutContext GetLayoutContext()
    {
        return new LayoutContext
        {
            Container = _layout,
            DefaultSpacing = DefaultSpacing,
            DefaultLabelSpacing = DefaultLabelSpacing,
            Invalidate = Invalidate,
            Scale = EtoPlatform.Current.GetLayoutScaleFactor(_window!)
        };
    }

    public event EventHandler? Invalidated;

    public Size GetSizeFor(LayoutElement element)
    {
        var context = GetLayoutContext();
        element.Materialize(context);
        return Size.Truncate(
            element.GetPreferredSize(
                context,
                new RectangleF(0, 0, MAX_SIZE, MAX_SIZE)));
    }
}