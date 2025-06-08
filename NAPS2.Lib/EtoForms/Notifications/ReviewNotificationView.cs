namespace NAPS2.EtoForms.Notifications;

public class ReviewNotificationView : LinkNotificationView
{
    private const string REVIEW_URL = "ms-windows-store://review/?ProductId=9N3QQ9W0B23Q";

    public ReviewNotificationView(ReviewNotification model)
        : base(model, MiscResources.ReviewPrompt, MiscResources.LeaveAReview, REVIEW_URL, null)
    {
        HideTimeout = HIDE_LONG;
    }

    protected override void LinkClick()
    {
        base.LinkClick();
        Manager!.Hide(Model);
    }
}