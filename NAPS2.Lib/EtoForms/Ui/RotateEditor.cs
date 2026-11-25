using Eto.Drawing;
using Eto.Forms;
using NAPS2.EtoForms.Widgets;

namespace NAPS2.EtoForms.Ui;

public class RotateEditor : UnaryImageEditor
{
    private const int MIN_LINE_DISTANCE = 50;

    private readonly SliderWithTextBox _angleSlider = new(new SliderWithTextBox.DecimalConstraints(-180, 180, 45, 1));

    private bool _guideExists;
    private PointF _guideStart, _guideEnd;

    public RotateEditor(Naps2Config config, UiImageList imageList, ThumbnailController thumbnailController,
        IIconProvider iconProvider) :
        base(config, imageList, thumbnailController)
    {
        Title = UiStrings.Rotate;
        IconName = "arrow_rotate_anticlockwise";
        HelpText = UiStrings.CustomRotationHelp;

        AttachIconDependency(scale =>
            _angleSlider.Icon = iconProvider.GetIcon("arrow_rotate_anticlockwise", IconScenario.Inline16, scale));
        Sliders = [_angleSlider];
    }

    protected override List<Transform> Transforms => [new RotationTransform((double) _angleSlider.DecimalValue)];

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _guideExists = true;
        _guideStart = _guideEnd = e.Location;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
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
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        Cursor = Cursors.Crosshair;
        _guideEnd = e.Location;
        Invalidate();
    }

    protected override void PaintOverlay(object? sender, PaintEventArgs e)
    {
        if (_guideExists)
        {
            e.Graphics.AntiAlias = true;
            e.Graphics.DrawLine(new Pen(new Color(0, 0, 0)), _guideStart, _guideEnd);
        }
    }
}