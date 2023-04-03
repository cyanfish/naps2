namespace NAPS2.EtoForms.Notifications;

public class DonateNotification : LinkNotification
{
    private const string DONATE_URL = "https://www.naps2.com/donate";

    public DonateNotification() : base(MiscResources.DonatePrompt, MiscResources.Donate, DONATE_URL, null)
    {
        HideTimeout = HIDE_LONG;
    }

    protected override void LinkClick()
    {
        base.LinkClick();
        Manager!.Hide(this);
    }
}