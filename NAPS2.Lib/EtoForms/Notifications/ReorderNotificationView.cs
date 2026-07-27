namespace NAPS2.EtoForms.Notifications;

public class ReorderNotificationView : LinkNotificationView
{
    private readonly UiImageList _imageList;
    
    public ReorderNotificationView(ReorderNotification model)
        : base(model, UiStrings.PagesReordered, UiStrings.Undo)
    {
        HideTimeout = HIDE_SHORT;
        _imageList = model.ImageList;
    }

    protected override void LinkClick()
    {
        _imageList.Undo().AssertNoAwait();
        Manager!.Hide(Model);
    }
}