using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Layout;

public class LayoutController
{
    private readonly Control _layout = EtoPlatform.Current.CreateContainer();
    private LayoutElement? _content;
    private Window? _window;
    private bool _firstLayout = true;

    public LayoutElement? Content
    {
        get => _content;
        set
        {
            _content = value;
            if (_window != null)
            {
                DoLayout();
            }
        }
    }

    public int RootPadding { get; set; } = 10;
    public int DefaultSpacing { get; set; } = 6;

    public void Bind(Window window)
    {
        if (Content == null) throw new InvalidOperationException();
        if (_window != null) throw new InvalidOperationException();
        _window = window;
        window.Content = _layout;
        window.Shown += (_, _) => DoLayout();
        window.SizeChanged += (_, _) => DoLayout();
    }

    private void DoLayout()
    {
        if (_window == null || _content == null) throw new InvalidOperationException();
        // TODO: Handle added/removed things
        var size = _window.ClientSize;
        var context = new LayoutContext(_layout)
        {
            DefaultSpacing = DefaultSpacing,
            IsFirstLayout = _firstLayout
        };
        _firstLayout = false;
        int p = RootPadding;
        var bounds = new Rectangle(p, p, size.Width - 2 * p, size.Height - 2 * p);
        if (LayoutElement.DEBUG_LAYOUT)
        {
            Debug.WriteLine("\n(((Starting layout)))");
        }
        _content.DoLayout(context, bounds);
    }
}