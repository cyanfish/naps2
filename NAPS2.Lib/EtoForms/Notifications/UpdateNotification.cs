using NAPS2.Update;

namespace NAPS2.EtoForms.Notifications;

public class UpdateNotification : NotificationModel
{
    public UpdateNotification(IUpdateChecker updateChecker, UpdateInfo update)
    {
        UpdateChecker = updateChecker;
        Update = update;
    }

    public IUpdateChecker UpdateChecker { get; }
    public UpdateInfo Update { get; }

    public override NotificationView CreateView()
    {
        return new UpdateNotificationView(this);
    }
}