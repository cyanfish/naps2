using Eto.Drawing;

namespace NAPS2.EtoForms.Widgets;

public class ManualDuplexListViewBehavior : ListViewBehavior<UiImage>
{
    private readonly UiThumbnailProvider _thumbnailProvider;

    public ManualDuplexListViewBehavior(ColorScheme colorScheme, UiThumbnailProvider thumbnailProvider) : base(colorScheme)
    {
        _thumbnailProvider = thumbnailProvider;
        MultiSelect = false;
    }

    public override bool ShowPageNumbers => true;

    public override Image GetImage(IListView<UiImage> listView, UiImage item)
    {
        using var thumbnail = _thumbnailProvider.GetThumbnail(item, listView.ImageSize.Width);
        return thumbnail.ToEtoImage();
    }
}
