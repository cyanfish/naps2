using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;

namespace NAPS2.EtoForms.Ui;

public class SplitForm : UnaryImageFormBase
{
    private const int HANDLE_WIDTH = 1;
    private const double HANDLE_RADIUS_RATIO = 0.2;
    private const int HANDLE_MIN_RADIUS = 50;

    private static CropTransform? _lastTransform;

    private readonly ColorScheme _colorScheme;
    private readonly Button _vSplit;
    private readonly Button _hSplit;

    // Mouse down location
    private PointF _mouseOrigin;

    // Crop amounts from each side as a fraction of the total image size (updated as the user drags)
    private float _cropX, _cropY;

    // Crop amounts from each side as pixels of the image to be cropped (updated on mouse up)
    private float _realX, _realY;

    private bool _dragging;
    private SplitOrientation _orientation;

    public SplitForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider, ColorScheme colorScheme) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Split;
        Icon = iconProvider.GetFormIcon("split_small");

        _colorScheme = colorScheme;

        _vSplit = C.IconButton(iconProvider.GetIcon("split_ver_small")!,
            () => SetOrientation(SplitOrientation.Vertical));
        _hSplit = C.IconButton(iconProvider.GetIcon("split_hor_small")!,
            () => SetOrientation(SplitOrientation.Horizontal));
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override List<Transform> Transforms => throw new NotSupportedException();

    private int HandleClickRadius =>
        (int) Math.Max(
            Math.Round((_orientation == SplitOrientation.Horizontal ? _overlayH : _overlayW) * HANDLE_RADIUS_RATIO),
            HANDLE_MIN_RADIUS);

    protected override void OnPreLoad(EventArgs e)
    {
        base.OnPreLoad(e);
        if (_lastTransform != null && _lastTransform.OriginalWidth == RealImageWidth &&
            _lastTransform.OriginalHeight == RealImageHeight)
        {
            _realX = _lastTransform.Left == 0 ? RealImageWidth / 2f : _lastTransform.Left;
            _realY = _lastTransform.Top == 0 ? RealImageHeight / 2f : _lastTransform.Top;
            _cropX = _realX / RealImageWidth;
            _cropY = _realY / RealImageHeight;
        }
        else
        {
            _cropX = 0.5f;
            _cropY = 0.5f;
            _realX = RealImageWidth / 2f;
            _realY = RealImageHeight / 2f;
        }
    }

    protected override LayoutElement CreateControls()
    {
        return L.Row(
            C.Filler(),
            L.Row(_vSplit, _hSplit),
            C.Filler()
        );
    }

    protected override void InitDisplayImage()
    {
        base.InitDisplayImage();
        _orientation = WorkingImage!.Width > WorkingImage!.Height
            ? SplitOrientation.Vertical
            : SplitOrientation.Horizontal;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        (_orientation == SplitOrientation.Horizontal ? _hSplit : _vSplit).Focus();
    }

    private void SetOrientation(SplitOrientation orientation)
    {
        _orientation = orientation;
        UpdatePreviewBox();
    }

    protected override IMemoryImage RenderPreview()
    {
        return WorkingImage!.Clone();
    }

    protected override void Revert()
    {
        _cropX = _cropY = 0.5f;
        _realX = RealImageWidth / 2f;
        _realY = RealImageHeight / 2f;
        Overlay.Invalidate();
    }

    private void Overlay_MouseDown(object? sender, MouseEventArgs e)
    {
        _dragging = IsHandleUnderMouse(e);
        _mouseOrigin = e.Location;
        Overlay.Invalidate();
    }

    private void Overlay_MouseUp(object? sender, MouseEventArgs e)
    {
        _realX = _cropX * RealImageWidth;
        _realY = _cropY * RealImageHeight;
        _dragging = false;
        Overlay.Invalidate();
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        Overlay.Cursor = _dragging || IsHandleUnderMouse(e)
            ? _orientation == SplitOrientation.Horizontal
                ? Cursors.HorizontalSplit
                : Cursors.VerticalSplit
            : Cursors.Arrow;
        if (_dragging)
        {
            UpdateCrop(e.Location);
            Overlay.Invalidate();
        }
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        base.PaintOverlay(sender, e);

        if (_overlayW == 0 || _overlayH == 0)
        {
            return;
        }

        var offsetX = _cropX * _overlayW;
        var offsetY = _cropY * _overlayH;
        var fillColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        var handlePen = new Pen(_colorScheme.CropColor, HANDLE_WIDTH);

        if (_overlayW >= 1 && _overlayH >= 1)
        {
            // Fade out cropped-out portions of the image
            if (_orientation == SplitOrientation.Horizontal)
            {
                e.Graphics.FillRectangle(fillColor, _overlayL, _overlayT + offsetY, _overlayW, _overlayH - offsetY);
            }
            else
            {
                e.Graphics.FillRectangle(fillColor, _overlayL + offsetX, _overlayT, _overlayW - offsetX, _overlayH);
            }
        }

        if (_orientation == SplitOrientation.Horizontal)
        {
            var y = _overlayT + offsetY - HANDLE_WIDTH / 2f;
            e.Graphics.DrawLine(handlePen, _overlayL, y, _overlayR - 1, y);
        }
        else
        {
            var x = _overlayL + offsetX - HANDLE_WIDTH / 2f;
            e.Graphics.DrawLine(handlePen, x, _overlayT, x, _overlayB - 1);
        }
    }

    private bool IsHandleUnderMouse(MouseEventArgs e)
    {
        var radius = HandleClickRadius;
        if (_orientation == SplitOrientation.Horizontal)
        {
            var y = _overlayT + _cropY * _overlayH;
            return e.Location.Y > y - radius && e.Location.Y < y + radius && e.Location.X > _overlayL &&
                   e.Location.X < _overlayR;
        }
        else
        {
            var x = _overlayL + _cropX * _overlayW;
            return e.Location.X > x - radius && e.Location.X < x + radius && e.Location.Y > _overlayT &&
                   e.Location.Y < _overlayB;
        }
    }

    private void UpdateCrop(PointF mousePos)
    {
        var delta = mousePos - _mouseOrigin;
        if (_orientation == SplitOrientation.Vertical)
        {
            _cropX = (_realX / RealImageWidth + delta.X / _overlayW)
                .Clamp(1f / RealImageWidth, (RealImageWidth - 1f) / RealImageWidth);
        }
        else
        {
            _cropY = (_realY / RealImageHeight + delta.Y / _overlayH)
                .Clamp(1f / RealImageHeight, (RealImageHeight - 1f) / RealImageHeight);
        }
    }

    protected override void Apply()
    {
        var transform1 = _orientation == SplitOrientation.Horizontal
            ? new CropTransform(0, 0, 0, RealImageHeight - (int) Math.Round(_realY), RealImageWidth, RealImageHeight)
            : new CropTransform(0, RealImageWidth - (int) Math.Round(_realX), 0, 0, RealImageWidth, RealImageHeight);
        var transform2 = _orientation == SplitOrientation.Horizontal
            ? new CropTransform(0, 0, (int) Math.Round(_realY), 0, RealImageWidth, RealImageHeight)
            : new CropTransform((int) Math.Round(_realX), 0, 0, 0, RealImageWidth, RealImageHeight);

        var thumb1 = WorkingImage!.Clone()
            .PerformAllTransforms([transform1, new ThumbnailTransform(ThumbnailController.RenderSize)]);
        var thumb2 = WorkingImage.Clone()
            .PerformAllTransforms([transform2, new ThumbnailTransform(ThumbnailController.RenderSize)]);

        // We keep the second image as the original UiImage reference so that any InsertAfter points come after the
        // pair of images. For example, if I'm in the middle of scanning and I split the most-recently scanned image,
        // the next scanned image should appear at the end of the list, not in between the split images.
        var oldTransforms = Image.TransformState;
        var image1 = new UiImage(Image.GetClonedImage());
        var image2 = Image;
        image1.AddTransform(transform1, thumb1);
        image2.AddTransform(transform2, thumb2);
        ImageList.Mutate(new ListMutation<UiImage>.InsertBefore(image1, image2));
        ImageList.AddToSelection(image1);
        ImageList.PushUndoElement(
            new SplitUndoElement(ImageList, image1, image2, oldTransforms, transform1, transform2));

        _lastTransform = transform2;
    }

    private enum SplitOrientation
    {
        Horizontal,
        Vertical
    }
}