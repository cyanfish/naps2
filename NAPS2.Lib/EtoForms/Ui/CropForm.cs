using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Ui;

public class CropForm : UnaryImageFormBase
{
    private static CropTransform? _lastTransform;

    private const int HANDLE_WIDTH = 3;
    private const double HANDLE_LENGTH_RATIO = 0.07;
    private const int HANDLE_MIN_LENGTH = 30;
    private const int FREEFORM_MIN_SIZE = 10;

    private readonly ColorScheme _colorScheme;

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

    public CropForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        ColorScheme colorScheme, IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Crop;
        Icon = new Icon(1f, iconProvider.GetIcon("transform_crop_small"));

        _colorScheme = colorScheme;

        OverlayBorderSize = HANDLE_WIDTH;
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    // The handle length is proportional to the window size
    private int HandleLength =>
        (int) Math.Max(Math.Round(Math.Min(_overlayH, _overlayW) * HANDLE_LENGTH_RATIO), HANDLE_MIN_LENGTH);

    protected override void OnPreLoad(EventArgs e)
    {
        base.OnPreLoad(e);
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
        Overlay.Invalidate();
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

    private void Overlay_MouseDown(object? sender, MouseEventArgs e)
    {
        _activeHandle = GetHandleUnderMouse(e);
        if (_activeHandle == Handle.None)
        {
            _freeformAvailable = true;
        }
        _mouseOrigin = e.Location;
        Overlay.Invalidate();
    }

    private Handle GetHandleUnderMouse(MouseEventArgs e)
    {
        // We calculate the distance between the mouse and each handle side
        // The 0.1 offset is to provide a bit of affinity so that if the crop size is 0 (so all distances are the same),
        // you can still e.g. pick the top-left handle if you put the mouse a bit top-left of it.
        var t = _overlayT + _realT * _overlayH / RealImageHeight;
        var b = _overlayB - _realB * _overlayH / RealImageHeight;
        var l = _overlayL + _realL * _overlayW / RealImageWidth;
        var r = _overlayR - _realR * _overlayW / RealImageWidth;
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
        }
        return Handle.None;
    }

    private void Overlay_MouseUp(object? sender, MouseEventArgs e)
    {
        _realT = _cropT * RealImageHeight;
        _realB = _cropB * RealImageHeight;
        _realL = _cropL * RealImageWidth;
        _realR = _cropR * RealImageWidth;
        _activeHandle = Handle.None;
        _freeformAvailable = false;
        _freeformActive = false;
        Overlay.Invalidate();
    }

    private void UpdateCrop(PointF mousePos)
    {
        var delta = mousePos - _mouseOrigin;
        if (_freeformActive)
        {
            var origin = _mouseOrigin - new PointF(_overlayL, _overlayT);
            var current = mousePos - new PointF(_overlayL, _overlayT);
            if (delta.Y > 0)
            {
                _cropT = (origin.Y / _overlayH).Clamp(0, 1);
                _cropB = (1 - current.Y / _overlayH).Clamp(0, 1);
            }
            else
            {
                _cropT = (current.Y / _overlayH).Clamp(0, 1);
                _cropB = (1 - origin.Y / _overlayH).Clamp(0, 1);
            }
            if (delta.X > 0)
            {
                _cropL = (origin.X / _overlayW).Clamp(0, 1);
                _cropR = (1 - current.X / _overlayW).Clamp(0, 1);
            }
            else
            {
                _cropL = (current.X / _overlayW).Clamp(0, 1);
                _cropR = (1 - origin.X / _overlayW).Clamp(0, 1);
            }
        }
        else
        {
            if (_activeHandle.HasFlag(Handle.Top))
            {
                _cropT = (_realT / RealImageHeight + delta.Y / _overlayH).Clamp(0, 1 - _cropB);
            }
            if (_activeHandle.HasFlag(Handle.Right))
            {
                _cropR = (_realR / RealImageWidth - delta.X / _overlayW).Clamp(0, 1 - _cropL);
            }
            if (_activeHandle.HasFlag(Handle.Bottom))
            {
                _cropB = (_realB / RealImageHeight - delta.Y / _overlayH).Clamp(0, 1 - _cropT);
            }
            if (_activeHandle.HasFlag(Handle.Left))
            {
                _cropL = (_realL / RealImageWidth + delta.X / _overlayW).Clamp(0, 1 - _cropR);
            }
        }
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        Overlay.Cursor = _freeformAvailable || (_activeHandle == Handle.None && GetHandleUnderMouse(e) == Handle.None)
            ? Cursors.Crosshair
            : Cursors.Pointer;
        if (_freeformAvailable && (Math.Abs(e.Location.Y - _mouseOrigin.Y) > FREEFORM_MIN_SIZE ||
                                   Math.Abs(e.Location.X - _mouseOrigin.X) > FREEFORM_MIN_SIZE))
        {
            _freeformActive = true;
        }
        UpdateCrop(e.Location);
        Overlay.Invalidate();
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        base.PaintOverlay(sender, e);

        if (_overlayW == 0 || _overlayH == 0)
        {
            return;
        }

        var offsetL = _cropL * _overlayW;
        var offsetT = _cropT * _overlayH;
        var offsetR = _cropR * _overlayW;
        var offsetB = _cropB * _overlayH;
        var fillColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        var handlePen = new Pen(_colorScheme.CropColor, HANDLE_WIDTH);

        if (_overlayW >= 1 && _overlayH >= 1)
        {
            // Fade out cropped-out portions of the image
            using var fade = new Bitmap((int) _overlayW, (int) _overlayH, PixelFormat.Format32bppRgba);
            var fadeGraphics = new Graphics(fade);
            fadeGraphics.FillRectangle(fillColor, 0, 0, _overlayW, _overlayH);
            fadeGraphics.SetClip(new RectangleF(
                offsetL, offsetT,
                _overlayW - offsetL - offsetR,
                _overlayH - offsetT - offsetB));
            fadeGraphics.Clear();
            fadeGraphics.Dispose();
            e.Graphics.DrawImage(fade, _overlayL, _overlayT);
        }

        var x1 = _overlayL + offsetL - HANDLE_WIDTH / 2f;
        var y1 = _overlayT + offsetT - HANDLE_WIDTH / 2f;
        var x2 = _overlayR - offsetR + HANDLE_WIDTH / 2f - 0.5f;
        var y2 = _overlayB - offsetB + HANDLE_WIDTH / 2f - 0.5f;
        var xMid = (x1 + x2) / 2;
        var yMid = (y1 + y2) / 2;

        // For a small crop selection, we shrink the handles so they don't overlap
        var xHandleLen = Math.Min(HandleLength, (x2 - x1) / 5);
        var yHandleLen = Math.Min(HandleLength, (y2 - y1) / 5);

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
        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right
    }
}