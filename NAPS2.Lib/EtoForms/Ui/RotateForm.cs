using Eto.Drawing;
using Eto.Forms;

namespace NAPS2.EtoForms.Ui;

public class RotateForm : ImageFormBase
{
    private const int MIN_LINE_DISTANCE = 50;

    private readonly SliderWithTextBox _angleSlider = new() { MinValue = -1800, MaxValue = 1800, TickFrequency = 450 };

    private bool _guideExists;
    private PointF _guideStart, _guideEnd;

    public RotateForm(Naps2Config config, ThumbnailController thumbnailController, IIconProvider iconProvider) :
        base(config, thumbnailController)
    {
        _angleSlider.Icon = iconProvider.GetIcon("arrow_rotate_anticlockwise_small");
        Sliders = new[] { _angleSlider };
        Overlay.Paint += Overlay_Paint;
        Overlay.MouseDown += Overlay_MouseDown;
        Overlay.MouseMove += Overlay_MouseMove;
        Overlay.MouseUp += Overlay_MouseUp;
    }

    protected override IEnumerable<Transform> Transforms =>
        new Transform[]
        {
            new RotationTransform(_angleSlider.Value / 10.0)
        };

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
            var oldAngle = _angleSlider.Value / 10.0;
            var newAngle = angle + oldAngle;
            while (newAngle > 180.0)
            {
                newAngle -= 360.0;
            }
            while (newAngle < -180.0)
            {
                newAngle += 360.0;
            }
            _angleSlider.Value = (int)Math.Round(newAngle * 10);
        }
        Overlay.Invalidate();
    }

    private void Overlay_MouseMove(object? sender, MouseEventArgs e)
    {
        Overlay.Cursor = Cursors.Crosshair;
        _guideEnd = e.Location;
        Overlay.Invalidate();
    }

    private void Overlay_Paint(object? sender, PaintEventArgs e)
    {
        if (_guideExists)
        {
            e.Graphics.AntiAlias = true;
            e.Graphics.DrawLine(new Pen(new Color(0, 0,0 )), _guideStart, _guideEnd);
        }
    }
}