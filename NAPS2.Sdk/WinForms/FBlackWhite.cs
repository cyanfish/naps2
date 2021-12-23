using System.Windows.Forms;

namespace NAPS2.WinForms;

partial class FBlackWhite : ImageForm
{
    public FBlackWhite(ImageContext imageContext)
        : base(imageContext)
    {
        InitializeComponent();
        ActiveControl = txtThreshold;
    }

    public BlackWhiteTransform BlackWhiteTransform { get; private set; } = new BlackWhiteTransform();

    protected override IEnumerable<Transform> Transforms => new[] { BlackWhiteTransform };

    protected override PictureBox PictureBox => pictureBox;

    protected override void ResetTransform()
    {
        BlackWhiteTransform = new BlackWhiteTransform();
        tbThreshold.Value = 0;
        txtThreshold.Text = tbThreshold.Value.ToString("G");
    }

    private void UpdateTransform()
    {
        BlackWhiteTransform = new BlackWhiteTransform(tbThreshold.Value);
        UpdatePreviewBox();
    }

    private void txtBlackWhite_TextChanged(object sender, EventArgs e)
    {
        if (int.TryParse(txtThreshold.Text, out int value))
        {
            if (value >= tbThreshold.Minimum && value <= tbThreshold.Maximum)
            {
                tbThreshold.Value = value;
            }
        }
        UpdateTransform();
    }

    private void tbBlackWhite_Scroll(object sender, EventArgs e)
    {
        txtThreshold.Text = tbThreshold.Value.ToString("G");
        UpdateTransform();
    }
}