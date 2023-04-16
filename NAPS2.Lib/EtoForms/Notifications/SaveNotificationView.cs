namespace NAPS2.EtoForms.Notifications;

public class SaveNotificationView : LinkNotificationView
{
    public SaveNotificationView(SaveNotification model)
        : base(model, model.Title, Path.GetFileName(model.Path), model.Path, Path.GetDirectoryName(model.Path))
    {
        HideTimeout = HIDE_SHORT;
    }
}