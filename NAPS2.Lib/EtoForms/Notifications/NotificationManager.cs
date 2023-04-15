using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public class NotificationManager
{
    private readonly Dictionary<Notification, LayoutElement> _items = new();

    public LayoutColumn Column { get; } = L.Column(C.Filler()).Spacing(20).Padding(15);

    public event EventHandler? Updated;

    public event EventHandler? TimersStarting;

    public void Show(Notification notification)
    {
        notification.Manager = this;
        var item = notification.CreateView();
        _items[notification] = item;
        Column.Children.Add(item);
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void Hide(Notification notification)
    {
        if (!_items.ContainsKey(notification)) return;
        Column.Children.Remove(_items[notification]);
        Updated?.Invoke(this, EventArgs.Empty);
        notification.Dispose();
    }

    public void StartTimers()
    {
        TimersStarting?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }
}