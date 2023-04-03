using NAPS2.Update;

namespace NAPS2.EtoForms.Notifications;

public class UpdateNotification : LinkNotification
{
    private readonly IUpdateChecker _updateChecker;
    private readonly UpdateInfo _update;

    public UpdateNotification(IUpdateChecker updateChecker, UpdateInfo update)
        : base(
            MiscResources.UpdateAvailable,
            string.Format(MiscResources.Install, update.Name),
            null, null)
    {
        _updateChecker = updateChecker;
        _update = update;
        HideTimeout = HIDE_LONG;
    }

    protected override void LinkClick()
    {
        _updateChecker.StartUpdate(_update);
        Manager!.Hide(this);
    }
}