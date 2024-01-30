namespace NAPS2.EtoForms.Layout;

public class LayoutVisibility
{
    public static LayoutVisibility? Combine(LayoutVisibility? first, LayoutVisibility? second)
    {
        if (first == null && second == null) return null;
        if (first == null) return second;
        if (second == null) return first;
        var vis = new LayoutVisibility(first.IsVisible && second.IsVisible);
        void VisHandler(object? sender, EventArgs e) => vis.IsVisible = first.IsVisible && second.IsVisible;
        first.IsVisibleChanged += VisHandler;
        second.IsVisibleChanged += VisHandler;
        return vis;
    }

    private bool _isVisible;

    public LayoutVisibility(bool isVisible)
    {
        _isVisible = isVisible;
    }

    public bool IsVisible
    {
        get => _isVisible; set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                IsVisibleChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public static LayoutVisibility operator !(LayoutVisibility value)
    {
        var negation = new LayoutVisibility(!value.IsVisible);
        value.IsVisibleChanged += (_, _) => negation.IsVisible = !value.IsVisible;
        negation.IsVisibleChanged += (_, _) => value.IsVisible = !negation.IsVisible;
        return negation;
    }

    public event EventHandler? IsVisibleChanged;
}