using Eto.Drawing;
using Eto.Forms;
using NAPS2.ImportExport.Images;

namespace NAPS2.EtoForms;

public class ImageListViewBehavior : ListViewBehavior<UiImage>
{
    private readonly UiThumbnailProvider _thumbnailProvider;
    private readonly ImageTransfer _imageTransfer;

    public ImageListViewBehavior(UiThumbnailProvider thumbnailProvider, ImageTransfer imageTransfer)
    {
        _thumbnailProvider = thumbnailProvider;
        _imageTransfer = imageTransfer;
        MultiSelect = true;
        ShowLabels = false;
        ScrollOnDrag = true;
        UseHandCursor = true;
    }

    public override Image GetImage(UiImage item, int imageSize)
    {
        return _thumbnailProvider.GetThumbnail(item, imageSize).ToEtoImage();
    }

    public override void SetDragData(ListSelection<UiImage> selection, IDataObject dataObject)
    {
        if (selection.Any())
        {
            using var selectedImages = selection.Select(x => x.GetClonedImage()).ToDisposableList();
            _imageTransfer.AddTo(dataObject, selectedImages.InnerList);
        }
    }

    public override DragEffects GetDropEffect(IDataObject dataObject)
    {
        try
        {
            if (_imageTransfer.IsIn(dataObject))
            {
                var data = _imageTransfer.GetFrom(dataObject);
                return data.ProcessId == Process.GetCurrentProcess().Id
                    ? DragEffects.Move
                    : DragEffects.Copy;
            }
            if (dataObject.Contains("FileDrop")) // TODO: Constant
            {
                return DragEffects.Copy;
            }
        }
        catch (Exception ex)
        {
            Log.ErrorException("Error receiving drag/drop", ex);
        }
        return DragEffects.None;
    }
}