using Eto.Drawing;

namespace NAPS2.EtoForms.Layout;

/// <summary>
/// A layout element whose content can be replaced after the layout is built. Controls from the previous content are
/// removed from the window on the next layout pass.
/// </summary>
public class LayoutPlaceholder : LayoutContainer
{
    private LayoutElement? _content;
    private Action? _invalidate;

    public LayoutPlaceholder(bool scale = false) : base([])
    {
        Scale = scale;
    }

    public LayoutElement? Content
    {
        get => _content;
        set
        {
            _content = value;
            Children.Clear();
            if (value != null)
            {
                Children.AddRange(ExpandChildren([value]));
            }
            _invalidate?.Invoke();
        }
    }

    public override void Materialize(LayoutContext context)
    {
        _invalidate = context.Invalidate;
        base.Materialize(context);
    }

    public override void DoLayout(LayoutContext context, RectangleF bounds)
    {
        foreach (var child in Children)
        {
            child.DoLayout(context, bounds);
        }
    }

    protected override SizeF GetPreferredSizeCore(LayoutContext context, RectangleF parentBounds)
    {
        var size = SizeF.Empty;
        foreach (var child in Children)
        {
            size = SizeF.Max(size, child.GetPreferredSize(context, parentBounds));
        }
        return size;
    }
}
