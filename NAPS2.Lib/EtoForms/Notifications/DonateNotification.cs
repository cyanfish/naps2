namespace NAPS2.EtoForms.Notifications;

public class DonateNotification : NotificationModel
{
    public override NotificationView CreateView()
    {
        return new DonateNotificationView(this);
    }
}