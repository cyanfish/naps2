namespace NAPS2.EtoForms.Notifications;

public class SaveNotification : NotificationModel
{
    public SaveNotification(string title, string path)
    {
        Title = title;
        Path = path;
    }

    public string Title { get; }
    public string Path { get; }

    public override NotificationView CreateView()
    {
        return new SaveNotificationView(this);
    }
}