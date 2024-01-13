namespace NAPS2.EtoForms.Notifications;

public class NotificationManager
{
    public NotificationManager(ColorScheme colorScheme)
    {
        ColorScheme = colorScheme;
    }

    public List<NotificationModel> Notifications { get; } = [];

    public ColorScheme ColorScheme { get; }

    public event EventHandler? Updated;

    public event EventHandler? TimersStarting;

    public void Show(NotificationModel notification)
    {
        Invoker.Current.Invoke(() =>
        {
            Notifications.Add(notification);
            Updated?.Invoke(this, EventArgs.Empty);
        });
    }

    public void Hide(NotificationModel notification)
    {
        Invoker.Current.Invoke(() =>
        {
            if (Notifications.Remove(notification))
            {
                Updated?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    public void StartTimers()
    {
        Invoker.Current.Invoke(() => TimersStarting?.Invoke(this, EventArgs.Empty));
    }

    public void InvokeUpdated()
    {
        Invoker.Current.Invoke(() => Updated?.Invoke(this, EventArgs.Empty));
    }
}