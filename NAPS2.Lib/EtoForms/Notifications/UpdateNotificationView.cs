using NAPS2.Update;

namespace NAPS2.EtoForms.Notifications;

public class UpdateNotificationView : LinkNotificationView
{
    private readonly IUpdateChecker _updateChecker;
    private readonly UpdateInfo _update;

    public UpdateNotificationView(UpdateNotification model)
        : base(
            model,
            MiscResources.UpdateAvailable,
            string.Format(MiscResources.Install, model.Update.Name),
            null, null)
    {
        _updateChecker = model.UpdateChecker;
        _update = model.Update;
        HideTimeout = HIDE_LONG;
    }

    protected override void LinkClick()
    {
        _updateChecker.StartUpdate(_update);
        Manager!.Hide(Model);
    }
}