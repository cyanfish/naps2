namespace NAPS2.EtoForms.Notifications;

public class ReorderNotification(UiImageList imageList) : NotificationModel
{
    public UiImageList ImageList { get; } = imageList;

    public override NotificationView CreateView()
    {
        return new ReorderNotificationView(this);
    }
}