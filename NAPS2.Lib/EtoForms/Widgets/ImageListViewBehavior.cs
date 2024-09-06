using Eto.Drawing;
using Eto.Forms;
using Google.Protobuf;
using NAPS2.ImportExport.Images;

namespace NAPS2.EtoForms.Widgets;

public class ImageListViewBehavior : ListViewBehavior<UiImage>
{
    private readonly UiThumbnailProvider _thumbnailProvider;
    private readonly Naps2Config _config;
    private readonly ImageTransfer _imageTransfer = new();

    public ImageListViewBehavior(UiThumbnailProvider thumbnailProvider,
        ColorScheme colorScheme, Naps2Config config) : base(colorScheme)
    {
        _thumbnailProvider = thumbnailProvider;
        _config = config;
        MultiSelect = true;
        ShowLabels = false;
        ScrollOnDrag = true;
        UseHandCursor = true;
        ScaleImageSize = true;
    }

    public override bool ShowPageNumbers => _config.Get(c => c.ShowPageNumbers);

    public override Image GetImage(IListView<UiImage> listView, UiImage item)
    {
        using var thumbnail = _thumbnailProvider.GetThumbnail(item, listView.ImageSize.Width);
        return thumbnail.ToEtoImage();
    }

    public override bool AllowDragDrop => true;

    public override bool AllowFileDrop => true;

    public override string CustomDragDataType => _imageTransfer.TypeName;

    public override byte[] SerializeCustomDragData(UiImage[] items)
    {
        using var processedImages = items.Select(x => x.GetClonedImage()).ToDisposableList();
        return _imageTransfer.ToBinaryData(processedImages);
    }

    public override DragEffects GetCustomDragEffect(byte[] data)
    {
        var dataObj = _imageTransfer.FromBinaryData(data);
        return dataObj.ProcessId == Process.GetCurrentProcess().Id
            ? DragEffects.Move
            : DragEffects.Copy;
    }

    public override byte[] MergeCustomDragData(byte[][] dataItems)
    {
        // TODO: Move to ImageTransfer?
        var mergedObj = new ImageTransferData();
        foreach (var data in dataItems)
        {
            var dataObj = _imageTransfer.FromBinaryData(data);
            if (mergedObj.ProcessId != 0 && mergedObj.ProcessId != dataObj.ProcessId)
            {
                throw new ArgumentException("Inconsistent process IDs in drag data");
            }
            mergedObj.ProcessId = dataObj.ProcessId;
            mergedObj.SerializedImages.AddRange(dataObj.SerializedImages);
        }
        return mergedObj.ToByteArray();
    }
}