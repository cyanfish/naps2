namespace NAPS2.EtoForms.Notifications;

public class ProgressNotification : NotificationModel
{
    public ProgressNotification(OperationProgress progress, IOperation op)
    {
        Progress = progress;
        Op = op;
    }

    public OperationProgress Progress { get; }
    public IOperation Op { get; }

    public override NotificationView CreateView()
    {
        return new ProgressNotificationView(this);
    }
}