using System.Windows.Forms;

namespace NAPS2.WinForms;

partial class FBrightnessContrast : ImageForm
{
    public FBrightnessContrast(ImageContext imageContext, BitmapRenderer bitmapRenderer)
        : base(imageContext, bitmapRenderer)
    {
        InitializeComponent();
        ActiveControl = txtBrightness;
    }

    public BrightnessTransform BrightnessTransform { get; private set; } = new BrightnessTransform();

    public TrueContrastTransform TrueContrastTransform { get; private set; } = new TrueContrastTransform();

    protected override IEnumerable<Transform> Transforms => new Transform[] { BrightnessTransform, TrueContrastTransform };

    protected override PictureBox PictureBox => pictureBox;
        
    protected override void ResetTransform()
    {
        BrightnessTransform = new BrightnessTransform();
        TrueContrastTransform = new TrueContrastTransform();
        tbBrightness.Value = 0;
        tbContrast.Value = 0;
        txtBrightness.Text = tbBrightness.Value.ToString("G");
        txtContrast.Text = tbContrast.Value.ToString("G");
    }

    private void UpdateTransform()
    {
        BrightnessTransform = new BrightnessTransform(tbBrightness.Value);
        TrueContrastTransform = new TrueContrastTransform(tbContrast.Value);
        UpdatePreviewBox();
    }

    private void txtBrightness_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtBrightness.Text, out int value))
        {
            if (value >= tbBrightness.Minimum && value <= tbBrightness.Maximum)
            {
                tbBrightness.Value = value;
            }
        }
        UpdateTransform();
    }

    private void tbBrightness_Scroll(object sender, EventArgs e)
    {
        txtBrightness.Text = tbBrightness.Value.ToString("G");
        UpdateTransform();
    }

    private void txtContrast_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtContrast.Text, out int value))
        {
            if (value >= tbContrast.Minimum && value <= tbContrast.Maximum)
            {
                tbContrast.Value = value;
            }
        }
        UpdateTransform();
    }

    private void tbContrast_Scroll(object sender, EventArgs e)
    {
        txtContrast.Text = tbContrast.Value.ToString("G");
        UpdateTransform();
    }
}