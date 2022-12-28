using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Ui;

public class CropForm : ImageFormBase
{
    private const int HANDLE_WIDTH = 3;
    private const int HANDLE_LENGTH = 20;

    // TODO: Textboxes for direct editing

    // Mouse down location
    private PointF _mouseOrigin;

    // Crop amounts from each side as a fraction of the total image size (updated as the user drags)
    private float _cropL, _cropR, _cropT, _cropB;

    // Crop amounts from each side as pixels of the image to be cropped (updated on mouse up)
    private float _realL, _realR, _realT, _realB;

    // Whether the given crop handle is currently being moved by the user (multiple can be true for corners)
    private bool _activeT, _activeL, _activeB, _activeR;

    public CropForm(Naps2Config config, ThumbnailController thumbnailController) :
        base(config, thumbnailController)
    {
        Icon = new Icon(1f, Icons.transform_crop.ToEtoImage());
        Title = UiStrings.Crop;

        OverlayBorderSize = HANDLE_WIDTH;
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override void ResetTransform()
    {
        _cropT = _cropL = _cropB = _cropR = _realT = _realL = _realB = _realR = 0;
        Overlay.Invalidate();
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new CropTransform(
                (int) Math.Round(_realL),
                (int) Math.Round(_realR),
                (int) Math.Round(_realT),
                (int) Math.Round(_realB),
                ImageWidth,
                ImageHeight)
        };

    private void Overlay_MouseDown(object? sender, MouseEventArgs e)
    {
        // We calculate the distance between the mouse and each handle side
        // The 0.1 offset is to provide a bit of affinity so that if the crop size is 0 (so all distances are the same),
        // you can still e.g. pick the top-left handle if you put the mouse a bit top-left of it.
        var t = _overlayT + _realT * _overlayH / ImageHeight;
        var b = _overlayB - _realB * _overlayH / ImageHeight;
        var l = _overlayL + _realL * _overlayW / ImageWidth;
        var r = _overlayR - _realR * _overlayW / ImageWidth;
        var dyT = Math.Abs(e.Location.Y - (t - 0.1f));
        var dyB = Math.Abs(e.Location.Y - (b + 0.1f));
        var dyM = Math.Abs(e.Location.Y - (t + b) / 2);
        var dxL = Math.Abs(e.Location.X - (l - 0.1f));
        var dxR = Math.Abs(e.Location.X - (r + 0.1f));
        var dxM = Math.Abs(e.Location.X - (l + r) / 2);
        var dxMin = Math.Min(Math.Min(dxL, dxR), dxM);
        var dyMin = Math.Min(Math.Min(dyT, dyB), dyM);
        if (dxMin < HANDLE_LENGTH && dyMin < HANDLE_LENGTH)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (dyT == dyMin && dxL == dxMin) _activeT = _activeL = true;
            else if (dyT == dyMin && dxR == dxMin) _activeT = _activeR = true;
            else if (dyB == dyMin && dxL == dxMin) _activeB = _activeL = true;
            else if (dyB == dyMin && dxR == dxMin) _activeB = _activeR = true;
            else if (dyT == dyMin && dxM == dxMin) _activeT = true;
            else if (dyM == dyMin && dxL == dxMin) _activeL = true;
            else if (dyB == dyMin && dxM == dxMin) _activeB = true;
            else if (dyM == dyMin && dxR == dxMin) _activeR = true;
            _mouseOrigin = e.Location;
        }
        Overlay.Invalidate();
    }

    private void Overlay_MouseUp(object? sender, MouseEventArgs e)
    {
        _realT = _cropT * ImageHeight;
        _realB = _cropB * ImageHeight;
        _realL = _cropL * ImageWidth;
        _realR = _cropR * ImageWidth;
        _activeT = _activeL = _activeB = _activeR = false;
        Overlay.Invalidate();
    }

    private void UpdateCrop(PointF mousePos)
    {
        var delta = mousePos - _mouseOrigin;
        if (_activeT)
        {
            _cropT = (_realT / ImageHeight + delta.Y / _overlayH).Clamp(0, 1 - _cropB);
        }
        if (_activeR)
        {
            _cropR = (_realR / ImageWidth - delta.X / _overlayW).Clamp(0, 1 - _cropL);
        }
        if (_activeB)
        {
            _cropB = (_realB / ImageHeight - delta.Y / _overlayH).Clamp(0, 1 - _cropT);
        }
        if (_activeL)
        {
            _cropL = (_realL / ImageWidth + delta.X / _overlayW).Clamp(0, 1 - _cropR);
        }
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        // TODO: Update cursor based on proximity to handle & state
        Overlay.Cursor = Cursors.Crosshair;
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
        var handlePen = new Pen(new Color(0, 0, 0), HANDLE_WIDTH);

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

        var x1 = _overlayL + offsetL - HANDLE_WIDTH / 2f;
        var y1 = _overlayT + offsetT - HANDLE_WIDTH / 2f;
        var x2 = _overlayR - offsetR + HANDLE_WIDTH / 2f - 0.5f;
        var y2 = _overlayB - offsetB + HANDLE_WIDTH / 2f - 0.5f;
        var xMid = (x1 + x2) / 2;
        var yMid = (y1 + y2) / 2;

        // Draw corner handles
        e.Graphics.DrawLines(handlePen,
            new PointF(x1, y1 + HANDLE_LENGTH),
            new PointF(x1, y1),
            new PointF(x1 + HANDLE_LENGTH, y1));
        e.Graphics.DrawLines(handlePen,
            new PointF(x1, y2 - HANDLE_LENGTH),
            new PointF(x1, y2),
            new PointF(x1 + HANDLE_LENGTH, y2));
        e.Graphics.DrawLines(handlePen,
            new PointF(x2, y1 + HANDLE_LENGTH),
            new PointF(x2, y1),
            new PointF(x2 - HANDLE_LENGTH, y1));
        e.Graphics.DrawLines(handlePen,
            new PointF(x2, y2 - HANDLE_LENGTH),
            new PointF(x2, y2),
            new PointF(x2 - HANDLE_LENGTH, y2));

        // Draw edge handles
        e.Graphics.DrawLine(handlePen, x1, yMid - HANDLE_LENGTH / 2f, x1, yMid + HANDLE_LENGTH / 2f);
        e.Graphics.DrawLine(handlePen, x2, yMid - HANDLE_LENGTH / 2f, x2, yMid + HANDLE_LENGTH / 2f);
        e.Graphics.DrawLine(handlePen, xMid - HANDLE_LENGTH / 2f, y1, xMid + HANDLE_LENGTH / 2f, y1);
        e.Graphics.DrawLine(handlePen, xMid - HANDLE_LENGTH / 2f, y2, xMid + HANDLE_LENGTH / 2f, y2);
    }
}