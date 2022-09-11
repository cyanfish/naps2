using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace NAPS2.WinForms;

partial class FRotate : ImageForm
{
    private const int MIN_LINE_DISTANCE = 50;
    private const float LINE_PEN_SIZE = 1;

    private bool _guideExists;
    private Point _guideStart, _guideEnd;

    public FRotate(ImageContext imageContext, ThumbnailController thumbnailController)
        : base(imageContext, thumbnailController)
    {
        InitializeComponent();
        txtAngle.Text += '\u00B0';
        ActiveControl = txtAngle;
    }

    public RotationTransform RotationTransform { get; private set; } = new RotationTransform();

    protected override IEnumerable<Transform> Transforms => new[] { RotationTransform };

    protected override PictureBox PictureBox => pictureBox;

    protected override void ResetTransform()
    {
        RotationTransform = new RotationTransform();
        tbAngle.Value = 0;
        txtAngle.Text = (tbAngle.Value / 10.0).ToString("G");
    }

    private void UpdateTransform()
    {
        RotationTransform = new RotationTransform(tbAngle.Value / 10.0);
        UpdatePreviewBox();
    }

    private void txtAngle_TextChanged(object sender, EventArgs e)
    {
        if (double.TryParse(txtAngle.Text.Replace('\u00B0'.ToString(CultureInfo.InvariantCulture), ""), out double valueDouble))
        {
            int value = (int)Math.Round(valueDouble * 10);
            if (value >= tbAngle.Minimum && value <= tbAngle.Maximum)
            {
                tbAngle.Value = value;
            }
            if (!txtAngle.Text.Contains('\u00B0'))
            {
                var (ss, sl) = (txtAngle.SelectionStart, txtAngle.SelectionLength);
                txtAngle.Text += '\u00B0';
                (txtAngle.SelectionStart, txtAngle.SelectionLength) = (ss, sl);
            }
        }
        UpdateTransform();
    }

    private void tbAngle_Scroll(object sender, EventArgs e)
    {
        txtAngle.Text = (tbAngle.Value / 10.0).ToString("G") + '\u00B0';
        UpdateTransform();
    }

    private void pictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        _guideExists = true;
        _guideStart = _guideEnd = e.Location;
        pictureBox.Invalidate();
    }

    private void pictureBox_MouseUp(object sender, MouseEventArgs e)
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
            var oldAngle = tbAngle.Value / 10.0;
            var newAngle = angle + oldAngle;
            while (newAngle > 180.0)
            {
                newAngle -= 360.0;
            }
            while (newAngle < -180.0)
            {
                newAngle += 360.0;
            }
            tbAngle.Value = (int)Math.Round(newAngle * 10);
            tbAngle_Scroll(null, null);
        }
        pictureBox.Invalidate();
    }

    private void pictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        _guideEnd = e.Location;
        pictureBox.Invalidate();
    }

    private void pictureBox_Paint(object sender, PaintEventArgs e)
    {
        if (_guideExists)
        {
            var old = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(new Pen(Color.Black, LINE_PEN_SIZE), _guideStart, _guideEnd);
            e.Graphics.SmoothingMode = old;
        }
    }
}