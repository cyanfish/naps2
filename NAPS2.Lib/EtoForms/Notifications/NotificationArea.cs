using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Notifications;

public class NotificationArea : IDisposable
{
    private readonly NotificationManager _manager;
    private readonly LayoutController _layoutController;
    private readonly Dictionary<NotificationModel, (NotificationView, LayoutElement)> _items = new();

    public NotificationArea(NotificationManager manager, LayoutController layoutController)
    {
        _manager = manager;
        _layoutController = layoutController;
        _manager.Updated += NotificationsUpdated;
        UpdateViews();
    }

    public LayoutColumn Content { get; } = L.Column(C.Filler()).Spacing(20).Padding(15);

    private void NotificationsUpdated(object? sender, EventArgs e)
    {
        UpdateViews();
        _layoutController.Invalidate();
    }

    private void UpdateViews()
    {
        foreach (var added in _manager.Notifications.Except(_items.Keys).ToList())
        {
            var view = added.CreateView();
            view.Manager = _manager;
            var content = view.CreateContent();
            Content.Children.Add(content);
            _items.Add(added, (view, content));
        }
        foreach (var removed in _items.Keys.Except(_manager.Notifications).ToList())
        {
            var (view, content) = _items[removed];
            Content.Children.Remove(content);
            view.Dispose();
        }
    }

    public void Dispose()
    {
        _manager.Updated -= NotificationsUpdated;
    }
}