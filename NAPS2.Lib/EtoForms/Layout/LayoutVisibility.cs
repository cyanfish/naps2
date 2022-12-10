namespace NAPS2.EtoForms.Layout;

public class LayoutVisibility
{
    private bool _isVisible;

    public LayoutVisibility(bool isVisible)
    {
        _isVisible = isVisible;
    }

    public bool IsVisible
    {
        get => _isVisible; set
        {
            _isVisible = value;
            IsVisibleChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? IsVisibleChanged;
}