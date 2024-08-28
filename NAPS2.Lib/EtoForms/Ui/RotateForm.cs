using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class RotateForm : UnaryImageFormBase
{
    private const int MIN_LINE_DISTANCE = 50;

    private readonly SliderWithTextBox _angleSlider = new(new SliderWithTextBox.DecimalConstraints(-180, 180, 45, 1));

    private bool _guideExists;
    private PointF _guideStart, _guideEnd;

    public RotateForm(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Rotate;
        IconName = "arrow_rotate_anticlockwise_small";

        _angleSlider.Icon = iconProvider.GetIcon("arrow_rotate_anticlockwise_small");
        Sliders = [_angleSlider];
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override List<Transform> Transforms => [new RotationTransform((double) _angleSlider.DecimalValue)];

    private void Overlay_MouseDown(object? sender, MouseEventArgs e)
    {
        _guideExists = true;
        _guideStart = _guideEnd = e.Location;
        Overlay.Invalidate();
    }

    private void Overlay_MouseUp(object? sender, MouseEventArgs e)
    {
        _guideExists = false;
        var dx = _guideEnd.X - _guideStart.X;
        var dy = _guideEnd.Y - _guideStart.Y;
        var distance = Math.Sqrt(dx * dx + dy * dy);
        if (distance > MIN_LINE_DISTANCE)
        {
            var angle = -Math.Atan2(dy, dx) * 180.0 / Math.PI;
            while (angle > 45.0)
            {
                angle -= 90.0;
            }
            while (angle < -45.0)
            {
                angle += 90.0;
            }
            var oldAngle = (double) _angleSlider.DecimalValue;
            var newAngle = angle + oldAngle;
            while (newAngle > 180.0)
            {
                newAngle -= 360.0;
            }
            while (newAngle < -180.0)
            {
                newAngle += 360.0;
            }
            _angleSlider.DecimalValue = (decimal) newAngle;
        }
        Overlay.Invalidate();
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        Overlay.Cursor = Cursors.Crosshair;
        _guideEnd = e.Location;
        Overlay.Invalidate();
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        base.PaintOverlay(sender, e);

        if (_guideExists)
        {
            e.Graphics.AntiAlias = true;
            e.Graphics.DrawLine(new Pen(new Color(0, 0, 0)), _guideStart, _guideEnd);
        }
    }
}