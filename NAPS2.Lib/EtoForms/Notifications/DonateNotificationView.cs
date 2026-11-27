namespace NAPS2.EtoForms.Notifications;

public class DonateNotificationView : LinkNotificationView
{
    private const string DONATE_URL = "https://www.naps2.com/donate?src=notif";

    public DonateNotificationView(DonateNotification model)
        : base(model, UiStrings.DonatePrompt, UiStrings.Donate, DONATE_URL, null)
    {
        HideTimeout = HIDE_LONG;
    }

    protected override void LinkClick()
    {
        base.LinkClick();
        Manager!.Hide(Model);
    }
}