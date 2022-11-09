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

    // Image bounds in the coordinate space of the overlay control
    private float _overlayT, _overlayL, _overlayR, _overlayB, _overlayW, _overlayH;

    // Crop amounts from each side as a fraction of the total image size (updated as the user drags)
    private float _cropL, _cropR, _cropT, _cropB;

    // Crop amounts from each side as pixels of the image to be cropped (updated on mouse up)
    private float _realL, _realR, _realT, _realB;

    // Whether the given crop handle is currently being moved by the user
    private bool _activeTL, _activeTR, _activeBL, _activeBR;

    public CropForm(Naps2Config config, ThumbnailController thumbnailController) :
        base(config, thumbnailController)
    {
        UseImageView = false;
        Overlay.Paint += Overlay_Paint;
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateImageCoords();
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        UpdateImageCoords();
    }

    private void UpdateImageCoords()
    {
        if (!Overlay.Loaded) return;
        var widthRatio = ImageWidth / (float) (Overlay.Width - HANDLE_WIDTH * 2);
        var heightRatio = ImageHeight / (float) (Overlay.Height - HANDLE_WIDTH * 2);
        var ratio = widthRatio / heightRatio;
        if (ratio > 1)
        {
            _overlayL = HANDLE_WIDTH;
            _overlayR = Overlay.Width - HANDLE_WIDTH * 2;
            var empty = Overlay.Height - Overlay.Height / ratio;
            _overlayT = empty / 2 + HANDLE_WIDTH;
            _overlayB = Overlay.Height - empty / 2 - HANDLE_WIDTH * 2;
        }
        else
        {
            _overlayT = HANDLE_WIDTH;
            _overlayB = Overlay.Height - HANDLE_WIDTH * 2;
            var empty = Overlay.Width - Overlay.Width * ratio;
            _overlayL = empty / 2 + HANDLE_WIDTH;
            _overlayR = Overlay.Width - empty / 2 - HANDLE_WIDTH * 2;
        }
        _overlayW = _overlayR - _overlayL;
        _overlayH = _overlayB - _overlayT;
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
        var dyt = Math.Abs(e.Location.Y - (_overlayT + _realT * _overlayH / ImageHeight));
        var dyb = Math.Abs(e.Location.Y - (_overlayB - _realB * _overlayW / ImageWidth));
        var dyl = Math.Abs(e.Location.X - (_overlayL + _realL * _overlayH / ImageHeight));
        var dyr = Math.Abs(e.Location.X - (_overlayR - _realR * _overlayW / ImageWidth));
        if (Math.Min(dyl, dyr) < HANDLE_LENGTH && Math.Min(dyt, dyb) < HANDLE_LENGTH)
        {
            if (dyt < dyb && dyl < dyr) _activeTL = true;
            if (dyt < dyb && dyr < dyl) _activeTR = true;
            if (dyb < dyt && dyl < dyr) _activeBL = true;
            if (dyb < dyt && dyr < dyl) _activeBR = true;
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
        _activeTL = _activeTR = _activeBL = _activeBR = false;
        Overlay.Invalidate();
    }

    private void UpdateCrop(PointF mousePos)
    {
        var delta = mousePos - _mouseOrigin;
        if (_activeTL || _activeTR)
        {
            _cropT = (_realT / ImageHeight + delta.Y / _overlayH).Clamp(0, 1);
        }
        if (_activeTR || _activeBR)
        {
            _cropR = (_realR / ImageWidth - delta.X / _overlayW).Clamp(0, 1);
        }
        if (_activeBL || _activeBR)
        {
            _cropB = (_realB / ImageHeight - delta.Y / _overlayH).Clamp(0, 1);
        }
        if (_activeTL || _activeBL)
        {
            _cropL = (_realL / ImageWidth + delta.X / _overlayW).Clamp(0, 1);
        }
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        // TODO: Update cursor based on proximity to handle & state
        Overlay.Cursor = Cursors.Crosshair;
        UpdateCrop(e.Location);
        Overlay.Invalidate();
    }

    private void Overlay_Paint(object? sender, PaintEventArgs e)
    {
        e.Graphics.DrawImage(WorkingImage!.ToEtoImage(), _overlayL, _overlayT, _overlayW, _overlayH);

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

        // TODO: Edge handles
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
    }
}