using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Layout;
using NAPS2.Images.Bitwise;

namespace NAPS2.EtoForms.Ui;

public class CropEditor : UnaryImageEditor
{
    private static CropTransform? _lastTransform;

    private const int HANDLE_WIDTH = 3;
    private const double HANDLE_LENGTH_RATIO = 0.07;
    private const int HANDLE_MIN_LENGTH = 30;
    private const int FREEFORM_MIN_SIZE = 10;

    private readonly ColorScheme _colorScheme;
    private readonly Button _autoCrop = C.Button(UiStrings.AutoCrop);

    // TODO: Textboxes for direct editing

    // Mouse down location
    private PointF _mouseOrigin;

    // Crop amounts from each side as a fraction of the total image size (updated as the user drags)
    private float _cropL, _cropR, _cropT, _cropB;

    // Crop amounts from each side as pixels of the image to be cropped (updated on mouse up)
    private float _realL, _realR, _realT, _realB;

    // The crop handle currently being moved by the user
    private Handle _activeHandle;

    private bool _freeformAvailable;
    private bool _freeformActive;

    public CropEditor(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        ColorScheme colorScheme, IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Crop;
        IconName = "transform_crop";
        HelpText = string.Format(UiStrings.CropHelp,
            EtoPlatform.Current.IsMac ? UiStrings.CommandKey : UiStrings.ControlKey,
            EtoPlatform.Current.IsMac ? UiStrings.OptionKey : UiStrings.AltKey,
            UiStrings.ShiftKey);

        _colorScheme = colorScheme;
        _autoCrop.Click += AutoCrop_Click;
    }

    protected override int ImagePadding => HANDLE_WIDTH;

    // The handle length is proportional to the window size
    private int HandleLength =>
        (int) Math.Max(Math.Round(Math.Min(ViewH, ViewW) * HANDLE_LENGTH_RATIO), HANDLE_MIN_LENGTH);

    protected override void OnInitialize()
    {
        base.OnInitialize();
        if (_lastTransform != null && _lastTransform.OriginalWidth == RealImageWidth &&
            _lastTransform.OriginalHeight == RealImageHeight)
        {
            _realL = _lastTransform.Left;
            _realR = _lastTransform.Right;
            _realT = _lastTransform.Top;
            _realB = _lastTransform.Bottom;
            _cropL = _realL / RealImageWidth;
            _cropR = _realR / RealImageWidth;
            _cropT = _realT / RealImageHeight;
            _cropB = _realB / RealImageHeight;
        }
    }

    protected override void Apply()
    {
        base.Apply();
        _lastTransform = (CropTransform) Transforms.Single();
    }

    protected override void Revert()
    {
        _cropT = _cropL = _cropB = _cropR = _realT = _realL = _realB = _realR = 0;
        Invalidate();
    }

    protected override LayoutElement CreateExtraButtons() => L.Row(base.CreateExtraButtons(), _autoCrop);

    private void AutoCrop_Click(object? sender, EventArgs e)
    {
        // The working image is scaled down to the screen size, which is too low resolution for reliable detection, so
        // we use the original image instead
        using var imageToRender = Image.GetClonedImage();
        using var image = imageToRender.Render();
        var op = new EdgeDetectionImageOp();
        op.Perform(image);
        _cropL = op.Left / (float) image.Width;
        _cropR = op.Right / (float) image.Width;
        _cropT = op.Top / (float) image.Height;
        _cropB = op.Bottom / (float) image.Height;
        UpdateRealCoords();
        Invalidate();
    }

    protected override IMemoryImage RenderPreview()
    {
        return WorkingImage!.Clone();
    }

    protected override List<Transform> Transforms =>
    [
        new CropTransform(
            (int) Math.Round(_realL),
            (int) Math.Round(_realR),
            (int) Math.Round(_realT),
            (int) Math.Round(_realB),
            RealImageWidth,
            RealImageHeight)
    ];

    protected override void OnKeyDown(KeyEventArgs e)
    {
        bool accelerate = e.Modifiers.HasFlag(Keys.Shift);
        bool moveOtherHandle = e.Modifiers.HasFlag(Application.Instance.CommonModifier);
        bool moveWhole = e.Modifiers.HasFlag(Keys.Alt);
        int d = accelerate ? 10 : 1;

        if (e.Key == Keys.Up)
        {
            if (moveWhole)
            {
                MoveWholeArea(0, -d);
            }
            else if (moveOtherHandle)
            {
                MoveCropTop(-d);
            }
            else
            {
                MoveCropBottom(-d);
            }
        }
        if (e.Key == Keys.Down)
        {
            if (moveWhole)
            {
                MoveWholeArea(0, d);
            }
            else if (moveOtherHandle)
            {
                MoveCropTop(d);
            }
            else
            {
                MoveCropBottom(d);
            }
        }
        if (e.Key == Keys.Left)
        {
            if (moveWhole)
            {
                MoveWholeArea(-d, 0);
            }
            else if (moveOtherHandle)
            {
                MoveCropLeft(-d);
            }
            else
            {
                MoveCropRight(-d);
            }
        }
        if (e.Key == Keys.Right)
        {
            if (moveWhole)
            {
                MoveWholeArea(d, 0);
            }
            else if (moveOtherHandle)
            {
                MoveCropLeft(d);
            }
            else
            {
                MoveCropRight(d);
            }
        }
        if (e.Key is Keys.Up or Keys.Down or Keys.Left or Keys.Right)
        {
            UpdateRealCoords();
            Invalidate();
            e.Handled = true;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _activeHandle = GetHandleUnderMouse(e);
        if (_activeHandle == Handle.None)
        {
            _freeformAvailable = true;
        }
        _mouseOrigin = e.Location;
        Invalidate();
    }

    private Handle GetHandleUnderMouse(MouseEventArgs e)
    {
        // We calculate the distance between the mouse and each handle side
        // The 0.1 offset is to provide a bit of affinity so that if the crop size is 0 (so all distances are the same),
        // you can still e.g. pick the top-left handle if you put the mouse a bit top-left of it.
        var t = ViewT + _realT * ViewH / RealImageHeight;
        var b = ViewB - _realB * ViewH / RealImageHeight;
        var l = ViewL + _realL * ViewW / RealImageWidth;
        var r = ViewR - _realR * ViewW / RealImageWidth;
        var dyT = Math.Abs(e.Location.Y - (t - 0.1f));
        var dyB = Math.Abs(e.Location.Y - (b + 0.1f));
        var dyM = Math.Abs(e.Location.Y - (t + b) / 2);
        var dxL = Math.Abs(e.Location.X - (l - 0.1f));
        var dxR = Math.Abs(e.Location.X - (r + 0.1f));
        var dxM = Math.Abs(e.Location.X - (l + r) / 2);
        var dxMin = Math.Min(Math.Min(dxL, dxR), dxM);
        var dyMin = Math.Min(Math.Min(dyT, dyB), dyM);
        // The user can click/drag the handle even if they miss a bit
        if (dxMin < HandleLength * 1.5 && dyMin < HandleLength * 1.5)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (dyT == dyMin && dxL == dxMin) return Handle.TopLeft;
            if (dyT == dyMin && dxR == dxMin) return Handle.TopRight;
            if (dyB == dyMin && dxL == dxMin) return Handle.BottomLeft;
            if (dyB == dyMin && dxR == dxMin) return Handle.BottomRight;
            if (dyT == dyMin && dxM == dxMin) return Handle.Top;
            if (dyM == dyMin && dxL == dxMin) return Handle.Left;
            if (dyB == dyMin && dxM == dxMin) return Handle.Bottom;
            if (dyM == dyMin && dxR == dxMin) return Handle.Right;
            if (dyM == dyMin && dxM == dxMin) return Handle.Middle;
        }
        return Handle.None;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        UpdateRealCoords();
        _activeHandle = Handle.None;
        _freeformAvailable = false;
        _freeformActive = false;
        Invalidate();
    }

    private void UpdateRealCoords()
    {
        _realT = _cropT * RealImageHeight;
        _realB = _cropB * RealImageHeight;
        _realL = _cropL * RealImageWidth;
        _realR = _cropR * RealImageWidth;
    }

    private void UpdateCrop(PointF mousePos)
    {
        var delta = mousePos - _mouseOrigin;
        if (_freeformActive)
        {
            var origin = _mouseOrigin - new PointF(ViewL, ViewT);
            var current = mousePos - new PointF(ViewL, ViewT);
            if (delta.Y > 0)
            {
                _cropT = (origin.Y / ViewH).Clamp(0, 1);
                _cropB = (1 - current.Y / ViewH).Clamp(0, 1);
            }
            else
            {
                _cropT = (current.Y / ViewH).Clamp(0, 1);
                _cropB = (1 - origin.Y / ViewH).Clamp(0, 1);
            }
            if (delta.X > 0)
            {
                _cropL = (origin.X / ViewW).Clamp(0, 1);
                _cropR = (1 - current.X / ViewW).Clamp(0, 1);
            }
            else
            {
                _cropL = (current.X / ViewW).Clamp(0, 1);
                _cropR = (1 - origin.X / ViewW).Clamp(0, 1);
            }
        }
        else
        {
            if (_activeHandle.HasFlag(Handle.Top))
            {
                MoveCropTop(delta.Y);
            }
            if (_activeHandle.HasFlag(Handle.Right))
            {
                MoveCropRight(delta.X);
            }
            if (_activeHandle.HasFlag(Handle.Bottom))
            {
                MoveCropBottom(delta.Y);
            }
            if (_activeHandle.HasFlag(Handle.Left))
            {
                MoveCropLeft(delta.X);
            }
            if (_activeHandle.HasFlag(Handle.Middle))
            {
                MoveWholeArea(delta.X, delta.Y);
            }
        }
    }

    private void MoveWholeArea(float deltaX, float deltaY)
    {
        // We move one handle, then make the other handle match.
        // Compared to moving both handles independently, this ensures the overall crop size doesn't change once
        // we hit the end of the image.
        float ySum = _cropB + _cropT;
        float xSum = _cropL + _cropR;
        if (deltaY < 0)
        {
            MoveCropTop(deltaY);
            _cropB = ySum - _cropT;
        }
        else if (deltaY > 0)
        {
            MoveCropBottom(deltaY);
            _cropT = ySum - _cropB;
        }
        if (deltaX < 0)
        {
            MoveCropLeft(deltaX);
            _cropR = xSum - _cropL;
        }
        else if (deltaX > 0)
        {
            MoveCropRight(deltaX);
            _cropL = xSum - _cropR;
        }
    }

    private void MoveCropTop(float deltaY)
    {
        _cropT = (_realT / RealImageHeight + deltaY / ViewH).Clamp(0, 1 - _cropB);
    }

    private void MoveCropRight(float deltaX)
    {
        _cropR = (_realR / RealImageWidth - deltaX / ViewW).Clamp(0, 1 - _cropL);
    }

    private void MoveCropBottom(float deltaY)
    {
        _cropB = (_realB / RealImageHeight - deltaY / ViewH).Clamp(0, 1 - _cropT);
    }

    private void MoveCropLeft(float deltaX)
    {
        _cropL = (_realL / RealImageWidth + deltaX / ViewW).Clamp(0, 1 - _cropR);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        Cursor = _freeformAvailable || (_activeHandle == Handle.None && GetHandleUnderMouse(e) == Handle.None)
            ? Cursors.Crosshair
            : Cursors.Pointer;
        if (_freeformAvailable && (Math.Abs(e.Location.Y - _mouseOrigin.Y) > FREEFORM_MIN_SIZE ||
                                   Math.Abs(e.Location.X - _mouseOrigin.X) > FREEFORM_MIN_SIZE))
        {
            _freeformActive = true;
        }
        UpdateCrop(e.Location);
        Invalidate();
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        if (ViewW == 0 || ViewH == 0)
        {
            return;
        }

        var offsetL = _cropL * ViewW;
        var offsetT = _cropT * ViewH;
        var offsetR = _cropR * ViewW;
        var offsetB = _cropB * ViewH;
        var fillColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        var handlePen = new Pen(_colorScheme.CropColor, HANDLE_WIDTH);

        // Fade out the cropped-out portions of the image, as one shape so there are no seams where separate fills
        // would meet
        using var fade = new GraphicsPath { FillMode = FillMode.Alternate };
        fade.AddRectangle(ViewL, ViewT, ViewW, ViewH);
        fade.AddRectangle(ViewL + offsetL, ViewT + offsetT, ViewW - offsetL - offsetR, ViewH - offsetT - offsetB);
        e.Graphics.FillPath(fillColor, fade);

        var x1 = ViewL + offsetL - HANDLE_WIDTH / 2f;
        var y1 = ViewT + offsetT - HANDLE_WIDTH / 2f;
        var x2 = ViewR - offsetR + HANDLE_WIDTH / 2f - 0.5f;
        var y2 = ViewB - offsetB + HANDLE_WIDTH / 2f - 0.5f;
        var xMid = (x1 + x2) / 2;
        var yMid = (y1 + y2) / 2;

        // For a small crop selection, we shrink the handles so they don't overlap
        var xHandleLen = Math.Min(HandleLength, (x2 - x1) / 5);
        var yHandleLen = Math.Min(HandleLength, (y2 - y1) / 5);
        var midHandleLen = Math.Min(xHandleLen, yHandleLen) / 3;

        if (_freeformActive)
        {
            // Draw border
            e.Graphics.DrawRectangle(new Pen(_colorScheme.CropColor), x1, y1, x2 - x1 - 1, y2 - y1 - 1);
        }
        else
        {
            // Draw corner handles
            e.Graphics.DrawLines(handlePen,
                new PointF(x1, y1 + yHandleLen),
                new PointF(x1, y1),
                new PointF(x1 + xHandleLen, y1));
            e.Graphics.DrawLines(handlePen,
                new PointF(x1, y2 - yHandleLen),
                new PointF(x1, y2),
                new PointF(x1 + xHandleLen, y2));
            e.Graphics.DrawLines(handlePen,
                new PointF(x2, y1 + yHandleLen),
                new PointF(x2, y1),
                new PointF(x2 - xHandleLen, y1));
            e.Graphics.DrawLines(handlePen,
                new PointF(x2, y2 - yHandleLen),
                new PointF(x2, y2),
                new PointF(x2 - xHandleLen, y2));

            // Draw edge handles
            e.Graphics.DrawLine(handlePen, x1, yMid - yHandleLen / 2f, x1, yMid + yHandleLen / 2f);
            e.Graphics.DrawLine(handlePen, x2, yMid - yHandleLen / 2f, x2, yMid + yHandleLen / 2f);
            e.Graphics.DrawLine(handlePen, xMid - xHandleLen / 2f, y1, xMid + xHandleLen / 2f, y1);
            e.Graphics.DrawLine(handlePen, xMid - xHandleLen / 2f, y2, xMid + xHandleLen / 2f, y2);

            // Draw middle handle
            e.Graphics.DrawLine(handlePen, xMid, yMid - midHandleLen / 2f, xMid, yMid + midHandleLen / 2f);
            e.Graphics.DrawLine(handlePen, xMid - midHandleLen / 2f, yMid, xMid + midHandleLen / 2f, yMid);
        }
    }

    [Flags]
    private enum Handle
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        Middle = 16,
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }
}